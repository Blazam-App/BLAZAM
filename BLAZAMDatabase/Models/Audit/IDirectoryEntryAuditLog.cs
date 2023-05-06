namespace BLAZAM.Database.Models.Audit
{
    public interface IDirectoryEntryAuditLog:ICommonAuditLog
    {
        string Sid { get; set; }
    }
}