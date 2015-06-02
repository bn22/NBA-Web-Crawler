using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Configuration;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.AppSettings["StorageConnectionString"]);
        private static CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
        private static CloudQueue urlQueue = queueClient.GetQueueReference("myurls");
        private static CloudStorageAccount storageAccount2 = CloudStorageAccount.Parse(
            ConfigurationManager.AppSettings["StorageConnectionString"]);
        private static CloudTableClient tableClient = storageAccount2.CreateCloudTableClient();
        private static CloudTable workerTable = tableClient.GetTableReference("worker");
        private static CloudTable startCommand = tableClient.GetTableReference("command");
        public webCrawler populateTable = new webCrawler();
        private int numberOfUrls = 0;

        public override void Run()
        {
            Trace.TraceInformation("WorkerRole1 is running");
            while (true)
            {
                try
                {
                    workerStatus newState = new workerStatus("Idle");
                    TableOperation insertOperation = TableOperation.Insert(newState);
                    workerTable.Execute(insertOperation);
                    String nextCommand = "";
                    TableQuery<command> lastestCommand = new TableQuery<command>().Take(1);
                    foreach (command entity in startCommand.ExecuteQuery(lastestCommand))
                    {
                        nextCommand = entity.RowKey.ToString();
                    }
                    if (nextCommand == "Restart")
                    {
                        populateTable = new webCrawler();                       
                        numberOfUrls = 0;
                        command newCommand1 = new command("Start");
                        TableOperation insertOperation2 = TableOperation.Insert(newCommand1);
                        startCommand.Execute(insertOperation2);
                    }
                    try
                    {
                        while (nextCommand == "Start")
                        {
                            CloudQueueMessage message1 = urlQueue.GetMessage();
                            String url = message1.AsString;
                            if (url.Contains("http://bleacherreport.com/robots.txt"))
                            {
                                workerStatus newState1 = new workerStatus("Loading BleacherReport");
                                TableOperation insertOperation1 = TableOperation.Insert(newState1);
                                workerTable.Execute(insertOperation1);
                                populateTable.readBleacherReport(url);
                            }
                            else if (url.Contains("/robots.txt"))
                            {
                                workerStatus newState2 = new workerStatus("Loading CNN");
                                TableOperation insertOperation2 = TableOperation.Insert(newState2);
                                workerTable.Execute(insertOperation2);
                                populateTable.readRobotText(url);
                            }
                            else
                            {
                                numberOfUrls = numberOfUrls + 1;
                                workerStatus newState3 = new workerStatus("Crawling");
                                TableOperation insertOperation3 = TableOperation.Insert(newState3);
                                workerTable.Execute(insertOperation3);
                                if (url.Contains("cnn.com") || url.Contains("bleacherreport.com"))
                                {
                                    storageAdder addToTable = new storageAdder(populateTable, numberOfUrls);
                                    populateTable = addToTable.readHTML(url);
                                }
                            }
                            TableQuery<command> lastestCommand1 = new TableQuery<command>().Take(1);
                            foreach (command entity in startCommand.ExecuteQuery(lastestCommand1))
                            {
                                nextCommand = entity.RowKey.ToString();
                            }
                            urlQueue.DeleteMessage(message1);
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        Debug.WriteLine(e);
                    }
                }
                catch (StorageException e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole1 has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}
