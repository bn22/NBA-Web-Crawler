using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole1
{
    public class errorMessage : TableEntity
    {
        public String url { get; set; }
        public String errorMessageContent { get; set; }

        public errorMessage(String url, String error)
        {
            this.PartitionKey = string.Format("{0:D19}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks); ;
            this.RowKey = Uri.EscapeDataString(url);
            this.errorMessageContent = error;
            this.url = url;
        }

        public errorMessage() { }
    }
}
