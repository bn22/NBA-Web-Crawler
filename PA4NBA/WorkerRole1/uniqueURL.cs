using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole1
{

    public class uniqueURL : TableEntity
    {
        public String url { get; set; }
        public int tableSize { get; set; }
        public int numberCrawled { get; set; }
        public String body { get; set; }

        public uniqueURL(String url)
        {
            this.PartitionKey = string.Format("{0:D19}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks);
            this.RowKey = Uri.EscapeDataString(url);
            this.url = url;
            this.tableSize = 0;
            this.numberCrawled = 0;
            this.body = null;
        }

        public uniqueURL() { }
    }
}
