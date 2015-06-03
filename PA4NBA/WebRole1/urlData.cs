using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRole1
{
    public class urlData : TableEntity
    {
        public String url { get; set; }
        public String title { get; set; }
        public DateTime lastModfiedDate { get; set; }


        public urlData(String url, DateTime date, String title)
        {
            this.PartitionKey = Uri.EscapeDataString(url);
            this.RowKey = Uri.EscapeDataString(title);
            this.title = title;
            this.lastModfiedDate = date;
            this.url = url;
        }

        public urlData() { }
    }
}