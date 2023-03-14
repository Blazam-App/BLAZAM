namespace BLAZAM.Database.Models.Database.Audit
{
    public interface ICommonAuditLog
    {
        string Action { get; set; }
        string? AfterAction { get; set; }
        string? BeforeAction { get; set; }
        int Id { get; set; }
        string? IpAddress { get; set; }
        string? Target { get; set; }
        DateTime Timestamp { get; set; }
        string Username { get; set; }
    }
}