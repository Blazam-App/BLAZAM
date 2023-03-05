using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Common.Models.Database.Audit
{
    public class EmailLog
    {
        public int EmailLogId {get;set;}
        /// <summary>
        /// The time the email was sent from the application
        /// </summary>
        public DateTime Sent { get; set; }

        public string? ServerResponse { get; set; }

        public string? To { get; set; }

        public string? Cc { get; set; }

        public string? Bcc { get; set; }

        public string? Subject { get; set; }

        public string? HtmlBody { get; set; }
    }
}
