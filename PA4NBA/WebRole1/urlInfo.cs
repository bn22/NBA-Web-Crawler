using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRole1
{
    public class urlInfo : TableEntity
    {
        public String url { get; set; }
        public String title { get; set; }
        public String memoryAvailable { get; set; }
        public String cpuUsage { get; set; }
        public String lastModifiedDate { get; set; }

        public urlInfo(String url, String fullTitle, String titleWord)
        {
            this.PartitionKey = titleWord;
            this.RowKey = Uri.EscapeDataString(url);
            this.title = fullTitle;
            this.url = url;
            this.memoryAvailable = null;
            this.cpuUsage = null;
            this.lastModifiedDate = DateTime.Now.ToString();
        }

        public urlInfo() { }
    }
}