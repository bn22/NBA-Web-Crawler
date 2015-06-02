using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRole1
{
    public class trieStatus : TableEntity
    {
        public String wikiTitle { get; set; }
        public int count { get; set; }

        public trieStatus(String title, int count)
        {
            this.PartitionKey = string.Format("{0:D19}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks);
            this.RowKey = title;
            this.count = count;
        }

        public trieStatus() { }
    }
}