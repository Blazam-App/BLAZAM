using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLAZAMDatabase.Models;

namespace BLAZAM.Database.Models.Audit
{
    internal class EmailLog : AppDbSetBase
    {
        public string? To { get; set; }

        public string? Cc { get; set; }
        public string? Bcc { get; set; }
        public string? Subject { get; set; }
        public string? HtmlBody { get; set; }
    }
}
