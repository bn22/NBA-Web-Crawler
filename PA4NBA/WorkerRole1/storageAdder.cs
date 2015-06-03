using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole1
{
    public class storageAdder
    {
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
           ConfigurationManager.AppSettings["StorageConnectionString"]);
        private static CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
        private static CloudQueue urlQueue = queueClient.GetQueueReference("myurls");
        private static CloudStorageAccount storageAccount2 = CloudStorageAccount.Parse(
            ConfigurationManager.AppSettings["StorageConnectionString"]);
        private static CloudTableClient tableClient = storageAccount2.CreateCloudTableClient();
        private static CloudTable table = tableClient.GetTableReference("crawledtable");
        private static CloudTable uniqueTable = tableClient.GetTableReference("urls");
        private static CloudTable errorTable = tableClient.GetTableReference("errors");
        public PerformanceCounter memProcess = new PerformanceCounter("Memory", "Available MBytes");
        public PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        public Boolean uniqueUrl { get; set; }
        public webCrawler wCrawler { get; set; }
        public int numberedCrawled { get; set; }

        public storageAdder(webCrawler crawler, int numberCrawl)
        {
            this.uniqueUrl = true;
            this.wCrawler = crawler;
            this.numberedCrawled = numberCrawl;
        }


        public webCrawler readHTML(String url)
        {
            uniqueUrl = wCrawler.uniqueCheck(url);
            if (uniqueUrl)
            {
                try
                {
                    WebClient client = new WebClient();
                    Stream readWebRequest = client.OpenRead(url);
                    using (StreamReader sr = new StreamReader(readWebRequest))
                    {
                        String line;
                        line = sr.ReadToEnd();
                        sr.Close();
                        var filteredResults = line.Split(new char[] {'>'}, StringSplitOptions.None);
                        var httpResult = filteredResults
                            .Where(x => x.Contains("/title") || x.Contains("a href") || x.Contains("lastmod") || x.Contains("og:description"))
                            .Select(x => x.ToString()).ToList();
                        String title = "";
                        String date = "";
                        String body = "";
                        foreach (String s in httpResult)
                        {
                            if (s.Contains("</title"))
                            {
                                String[] splitTitle = s.Split(new char[] {'<'});
                                title = splitTitle[0];
                            }
                            else if (s.Contains("a href=\"http:"))
                            {
                                String[] splitLink = s.Split(new char[] {'\"'});
                                CloudQueueMessage newLink = new CloudQueueMessage(splitLink[1]);
                                urlQueue.AddMessage(newLink);
                            }
                            else if (s.Contains("lastmod"))
                            {
                                String[] splitDate = s.Split(new String[] {"\""}, StringSplitOptions.None);
                                date = splitDate[1];
                            }
                            else if (s.Contains("og:description"))
                            {
                                String[] splitBody = s.Split(new char[] {'"'}, StringSplitOptions.None);
                                body = Uri.UnescapeDataString(splitBody[1]);
                            }
                        }
                        String[] titleWords = title.Split(new char[] { '.', ':', ',', '"', ';', '-', ')', ' ', '(', '!'});
                        wCrawler.visitedURL.Add(url);
                        uniqueURL newURL = new uniqueURL(url);
                        newURL.url = url;
                        newURL.numberCrawled = numberedCrawled;
                        newURL.tableSize = wCrawler.visitedURL.Count();
                        newURL.body = body;
                        TableOperation insertOperation8 = TableOperation.Insert(newURL);
                        uniqueTable.Execute(insertOperation8);
                        for (int i = 0; i < titleWords.Length; i++)
                        {
                            String currentWord = titleWords[i];
                            if (!currentWord.Equals(" ") || !currentWord.Equals(""))
                            {                               
                                float memUsage = memProcess.NextValue();
                                String memAvailable = memUsage.ToString();
                                float cpuPercent = cpuCounter.NextValue();
                                String cpuPercentage = cpuPercent.ToString();
                                urlInfo addItem = new urlInfo(url, title, currentWord.ToLower());
                                addItem.url = url;
                                addItem.title = title.Replace("'", "");
                                addItem.memoryAvailable = memAvailable;
                                addItem.cpuUsage = cpuPercentage;
                                if (date != null)
                                {
                                    addItem.lastModifiedDate = date;
                                }
                                else
                                {
                                    addItem.lastModifiedDate = DateTime.Now.ToString();
                                }
                                TableOperation insertOperation2 = TableOperation.Insert(addItem);
                                table.Execute(insertOperation2);
                            }
                        }
                    }
                }
                catch (WebException e)
                {
                    String error = e.ToString();
                    errorMessage currentError = new errorMessage(url, error);
                    currentError.url = url;
                    currentError.errorMessageContent = error;
                    TableOperation insertOperation9 = TableOperation.Insert(currentError);
                    errorTable.Execute(insertOperation9);
                }
            }
            return wCrawler;
        }
        public storageAdder() { }
    }
}
