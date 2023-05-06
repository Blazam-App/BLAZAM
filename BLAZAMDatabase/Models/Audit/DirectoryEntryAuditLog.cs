namespace BLAZAM.Database.Models.Audit
{

    public class DirectoryEntryAuditLog : CommonAuditLog, IDirectoryEntryAuditLog
    {
        public string Sid { get; set; }
    }
}
