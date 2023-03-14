using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Database.Models.Database.Audit
{
    internal class EmailLog
    {
        public int EmailLogId {get;set;}
        public string? To { get; set; }

        public string? Cc { get; set; }
        public string? Bcc { get; set; }
        public string? Subject { get; set; }
        public string? HtmlBody { get; set; }
    }
}
