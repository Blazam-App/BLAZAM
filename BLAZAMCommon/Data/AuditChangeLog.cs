namespace BLAZAM.Common.Data
{
    public class AuditChangeLog
    {
        public string Field { get; set; }
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
    }
}