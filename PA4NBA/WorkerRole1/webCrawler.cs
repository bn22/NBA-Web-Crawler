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
    public class webCrawler
    {
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
           ConfigurationManager.AppSettings["StorageConnectionString"]);
        private static CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
        private static CloudQueue urlQueue = queueClient.GetQueueReference("myurls");
        private static CloudStorageAccount storageAccount2 = CloudStorageAccount.Parse(
            ConfigurationManager.AppSettings["StorageConnectionString"]);
        private static CloudTableClient tableClient = storageAccount2.CreateCloudTableClient();
        private static CloudTable errorTable = tableClient.GetTableReference("errors");
        private static CloudTable workerTable = tableClient.GetTableReference("worker");
        public List<String> visitedURL { get; set; }
        public List<String> disallowedURL { get; set; }
        private DateTime cutoff;

        public webCrawler()
        {
            this.visitedURL = new List<String>();
            this.disallowedURL = new List<String>();
            this.cutoff = new DateTime(2015, 4, 1);
        }

        public void readRobotText(String url)
        {
            WebClient client = new WebClient();
            Stream readWebRequest = client.OpenRead(url);
            using (StreamReader sr = new StreamReader(readWebRequest))
            {
                try
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Contains("Disallow: "))
                        {
                            String[] disallowedUrl = line.Split(' ');
                            disallowedURL.Add(disallowedUrl[1] + "/");
                        }
                        else if (line.Contains("Sitemap: "))
                        {
                            String[] sitemapUrl = line.Split(' ');
                            readXML(sitemapUrl[1]);
                        }
                    }
                    sr.Close();
                }
                catch (WebException e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        public void readBleacherReport(String url)
        {
            readXML("http://bleacherreport.com/sitemap/nba.xml");
        }

        public void readXML(String url)
        {
            WebClient client = new WebClient();
            Stream readWebRequest = client.OpenRead(url);
            using (StreamReader sr = new StreamReader(readWebRequest))
            {
                try
                {
                    String line;
                    line = sr.ReadToEnd();
                    sr.Close();
                    var filteredResults = line.Split(new String[] { "<sitemap" }, StringSplitOptions.None);
                    if (filteredResults.Count() == 1 || filteredResults.Count() == 0)
                    {
                        filteredResults = line.Split(new String[] { "<url" }, StringSplitOptions.None);
                    }
                    var httpResult = filteredResults
                        .Where(x => x.Contains("<loc>"))
                        .Select(x => x.ToString()).ToList();
                    foreach (String s in httpResult)
                    {
                        Boolean pass = true;
                        String[] splitLine = s.Split(new char[] { '>', '<' });
                        if (splitLine[7].StartsWith("2015"))
                        {
                            if (checkDate(splitLine[7]) == false)
                            {
                                pass = false;
                            }
                        }
                        if (s.Contains(".xml"))
                        {
                            if (pass)
                            {
                                readXML(splitLine[3]);
                            }
                        }
                        else
                        {
                            if (pass)
                            {
                                CloudQueueMessage m = new CloudQueueMessage(splitLine[3]);
                                urlQueue.AddMessage(m);
                            }
                        }
                    }
                }
                catch (WebException e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
        }

        public Boolean checkDate(String url)
        {
            DateTime lastModified = Convert.ToDateTime(url);
            if (lastModified > cutoff)
            {
                return true;
            }
            return false;
        }

        public Boolean uniqueCheck(String givenUrl)
        {
            foreach (String s in disallowedURL)
            {
                if (givenUrl.Contains(s))
                {
                    String error2 = "This link was disallowed";
                    errorMessage currentError = new errorMessage(givenUrl, error2);
                    currentError.url = givenUrl;
                    currentError.errorMessageContent = error2;
                    TableOperation insertOperation7 = TableOperation.Insert(currentError);
                    errorTable.Execute(insertOperation7);
                    return false;
                }
            }
            foreach (String st in visitedURL)
            {
                if (st.Contains(givenUrl))
                {
                    String error1 = "This link was already visited";
                    errorMessage currentError = new errorMessage(givenUrl, error1);
                    currentError.url = givenUrl;
                    currentError.errorMessageContent = error1;
                    TableOperation insertOperation3 = TableOperation.Insert(currentError);
                    errorTable.Execute(insertOperation3);
                    return false;
                }
            }
            return true;
        }
    }
}
