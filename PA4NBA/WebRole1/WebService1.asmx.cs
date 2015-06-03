using System.Web.UI.WebControls;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;

namespace WebRole1
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
              ConfigurationManager.AppSettings["StorageConnectionString"]);
        private static CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
        private static CloudQueue urlQueue = queueClient.GetQueueReference("myurls");
        private static CloudStorageAccount storageAccount2 = CloudStorageAccount.Parse(
            ConfigurationManager.AppSettings["StorageConnectionString"]);
        private static CloudTableClient tableClient = storageAccount2.CreateCloudTableClient();
        private static CloudTable table = tableClient.GetTableReference("crawledtable");
        private static CloudTable error = tableClient.GetTableReference("errors");
        private static CloudTable workerTable = tableClient.GetTableReference("worker");
        private static CloudTable startCommand = tableClient.GetTableReference("command");
        private static CloudTable trieTable = tableClient.GetTableReference("trie");
        private static CloudTable uniqueTable = tableClient.GetTableReference("urls");
        private static CloudTable errorTable = tableClient.GetTableReference("errors");
        private PerformanceCounter memProcess = new PerformanceCounter("Memory", "Available MBytes");
        private static trie wikiTitles = new trie();
        private static String filepath;
        private static Dictionary<string, List<String>> dynamicCache = new Dictionary<string, List<String>>();


        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void beginCrawler()
        {
            dynamicCache = new Dictionary<string, List<String>>();
            uniqueTable.CreateIfNotExists();
            table.CreateIfNotExists();
            error.CreateIfNotExists();
            workerTable.CreateIfNotExists();
            startCommand.CreateIfNotExists();
            urlQueue.CreateIfNotExists();
            String robotText = "http://www.cnn.com/robots.txt";
            command newCommand1 = new command("Restart");
            TableOperation insertOperation2 = TableOperation.Insert(newCommand1);
            startCommand.Execute(insertOperation2);
            CloudQueueMessage message2 = new CloudQueueMessage(robotText);
            urlQueue.AddMessage(message2);
            if (robotText.Contains("http://www.cnn.com/robots.txt"))
            {
                CloudQueueMessage message3 = new CloudQueueMessage("http://bleacherreport.com/robots.txt");
                urlQueue.AddMessage(message3);
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void endCrawler()
        {
            CloudQueueMessage message3 = new CloudQueueMessage("End");
            command newCommand = new command("End");
            TableOperation insertOperation4 = TableOperation.Insert(newCommand);
            startCommand.Execute(insertOperation4);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void clearCrawler()
        {
            urlQueue.Clear();
            uniqueTable.DeleteIfExists();
            table.DeleteIfExists();
            startCommand.DeleteIfExists();
            error.DeleteIfExists();
            workerTable.DeleteIfExists();
            System.Threading.Thread.Sleep(2000);
        }

        [WebMethod]
        public void downloadWiki()
        {
            trieTable.DeleteIfExists();
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("blob");
            CloudBlockBlob blockBlob2 = container.GetBlockBlobReference("wikiTitles.txt");
            if (container.Exists())
            {
                foreach (IListBlobItem item in container.ListBlobs(null, false))
                {
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        CloudBlockBlob blob = (CloudBlockBlob)item;
                        filepath = System.IO.Path.GetTempFileName();
                        using (var fileStream = System.IO.File.OpenWrite(filepath))
                        {
                            blockBlob2.DownloadToStream(fileStream);
                        }
                    }
                }
            }
        }

        [WebMethod]
        public void buildTrie()
        {
            trieTable.CreateIfNotExists();
            int lineCount = 0;
            try
            {
                using (StreamReader sr = new StreamReader(filepath))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (lineCount % 2000 == 0)
                        {
                            float memUsage = memProcess.NextValue();
                            if (memUsage < 5)
                            {
                                break;
                            }
                        }
                        else
                        {
                            wikiTitles.addTitles(line);
                        }
                        lineCount = lineCount + 1;
                        trieStatus update2 = new trieStatus(line, lineCount);
                        TableOperation insertOperation8 = TableOperation.Insert(update2);
                        trieTable.Execute(insertOperation8);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public String searchTrie(String prefix)
        {
            List<String> results = new List<String>();
            results = wikiTitles.findSuggestions(prefix);
            return new JavaScriptSerializer().Serialize(results);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public String findCNNArticles(String userInput)
        {
            List<String> queryResults = new List<string>();
            List<urlData> linqHelper = new List<urlData>();
            userInput = userInput.ToLower();
            if (dynamicCache.ContainsKey(userInput))
            {
                queryResults = dynamicCache[userInput];
            }
            else
            {
                String[] wordsInUserInput = userInput.Split(' ');
                for (int i = 0; i < wordsInUserInput.Length; i++)
                {
                    TableQuery<urlInfo> lastestQuery = new TableQuery<urlInfo>()
                        .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                            wordsInUserInput[i]));
                    foreach (urlInfo entity in table.ExecuteQuery(lastestQuery))
                    {
                        DateTime lastmod = new DateTime();
                        String rightNow = entity.lastModifiedDate.ToString();
                        if (rightNow.Equals(""))
                        {
                            lastmod = new DateTime(2015, 4, 1);
                        }
                        else
                        {
                            lastmod = Convert.ToDateTime(rightNow);
                        }
                        urlData info = new urlData(Uri.EscapeDataString(entity.RowKey.ToString()), lastmod, entity.title.ToString());
                        linqHelper.Add(info);
                    }
                }
                var filteredResults = linqHelper
                    .GroupBy(x => x.PartitionKey)
                    .Select(
                        x =>
                            new Tuple<String, int, DateTime, string>(x.ElementAt(0).url, x.ToList().Count,
                                x.ElementAt(0).lastModfiedDate, x.ElementAt(0).title))
                    .OrderByDescending(x => x.Item2)
                    .ThenByDescending(x => x.Item3);
                foreach (var s in filteredResults)
                {
                    if (queryResults.Count < 10)
                    {
                        String newResult = s.ToString();
                        String[] filteredResult = newResult.Split(',');
                        String add = filteredResult[0].Replace('(', ' ').Trim();
                        queryResults.Add(add);                   
                    }
                }
                if (dynamicCache.Count > 100)
                {
                    Random r = new Random();
                    int ranNumber = r.Next(100);
                    var findKeys = dynamicCache.Keys.ToList();
                    String pleaseFindThisKey = findKeys[ranNumber];
                    dynamicCache.Remove(pleaseFindThisKey);
                }
                dynamicCache.Add(userInput, queryResults);
            }
            return new JavaScriptSerializer().Serialize(queryResults);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public String cpu()
        {
            List<String> results = new List<string>();
            TableQuery<urlInfo> lastestQuery = new TableQuery<urlInfo>().Take(1);
            foreach (urlInfo entity in table.ExecuteQuery(lastestQuery))
            {
                results.Add(entity.cpuUsage.ToString());
            }
            return new JavaScriptSerializer().Serialize(results);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public String memUsage()
        {
            List<String> results = new List<string>();
            TableQuery<urlInfo> lastestQuery = new TableQuery<urlInfo>().Take(1);
            foreach (urlInfo entity in table.ExecuteQuery(lastestQuery))
            {
                results.Add(entity.memoryAvailable.ToString());
            }
            return new JavaScriptSerializer().Serialize(results);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public String lastTenUrl()
        {
            List<String> results = new List<string>();
            TableQuery<uniqueURL> lastestQuery = new TableQuery<uniqueURL>().Take(10);
            foreach (uniqueURL entity in uniqueTable.ExecuteQuery(lastestQuery))
            {
                results.Add(entity.url.ToString());
            }
            return new JavaScriptSerializer().Serialize(results);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public String queueCount()
        {
            urlQueue.FetchAttributes();
            int? numberInQueue = urlQueue.ApproximateMessageCount;
            String results = numberInQueue.ToString();
            return new JavaScriptSerializer().Serialize(results);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public String workerState()
        {
            List<String> result = new List<String>();
            TableQuery<workerStatus> newQuery = new TableQuery<workerStatus>().Take(1);
            foreach (workerStatus entity in workerTable.ExecuteQuery(newQuery))
            {
                result.Add(entity.RowKey.ToString());
            }
            return new JavaScriptSerializer().Serialize(result);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public String checksPerformed()
        {
            List<String> results = new List<string>();
            TableQuery<uniqueURL> lastestQuery = new TableQuery<uniqueURL>().Take(1);
            foreach (uniqueURL entity in uniqueTable.ExecuteQuery(lastestQuery))
            {
                results.Add(entity.numberCrawled.ToString());
            }
            return new JavaScriptSerializer().Serialize(results);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public String checksPassed()
        {
            List<String> results = new List<string>();
            TableQuery<uniqueURL> lastestQuery = new TableQuery<uniqueURL>().Take(1);
            foreach (uniqueURL entity in uniqueTable.ExecuteQuery(lastestQuery))
            {
                results.Add(entity.tableSize.ToString());
            }
            return new JavaScriptSerializer().Serialize(results);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public String lastTitle()
        {
            List<String> result = new List<String>();
            TableQuery<trieStatus> newQuery = new TableQuery<trieStatus>().Take(1);
            foreach (trieStatus entity in trieTable.ExecuteQuery(newQuery))
            {
                result.Add(entity.RowKey.ToString());
            }
            return new JavaScriptSerializer().Serialize(result);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public String numberOfTitles()
        {
            List<String> result = new List<String>();
            TableQuery<trieStatus> newQuery = new TableQuery<trieStatus>().Take(1);
            foreach (trieStatus entity in trieTable.ExecuteQuery(newQuery))
            {
                result.Add(entity.count.ToString());
            }
            return new JavaScriptSerializer().Serialize(result);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public String lastTenErrors()
        {
            List<String> results = new List<string>();
            TableQuery<errorMessage> lastestQuery = new TableQuery<errorMessage>().Take(10);
            foreach (errorMessage entity in errorTable.ExecuteQuery(lastestQuery))
            {
                results.Add(entity.url.ToString());
            }
            return new JavaScriptSerializer().Serialize(results);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public String findErrorMessage()
        {
            List<String> results = new List<string>();
            TableQuery<errorMessage> lastestQuery = new TableQuery<errorMessage>().Take(10);
            foreach (errorMessage entity in errorTable.ExecuteQuery(lastestQuery))
            {
                results.Add(entity.errorMessageContent.ToString());
            }
            return new JavaScriptSerializer().Serialize(results);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public String trieStatus()
        {
            List<String> results = new List<String>();
            if (wikiTitles.isEmpty() == true)
            {
                results.Add("Not Active");
            }
            else
            {
                results.Add("Active");
            }
            return new JavaScriptSerializer().Serialize(results);
        }
    }
}
