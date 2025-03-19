

using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Database.Models.Audit
{
    public class EmailAuditLog : AppDbSetBase
    {
        public string? MessageGuid { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }

        public string? Cc { get; set; }
        public string? Bcc { get; set; }
        public string? Subject { get; set; }
        public string? HtmlBody { get; set; }
        public bool IsRead { get; set; }
        public DateTime LastAttemptTimestamp { get; set; }
        public int Retries { get; set; }
        public DateTime? ReadTimestamp { get; set; }
        public string? ServerResponse { get; set; }

        [NotMapped]
        public bool Delivered { get { return ServerResponse?.StartsWith("2") == true; } }
    }
}
