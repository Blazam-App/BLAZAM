namespace BLAZAM.Common.Data.ActiveDirectory.Models
{
    public class AuditChangeLog
    {
        public string Field { get; internal set; }
        public object? OldValue { get; internal set; }
        public object? NewValue { get; internal set; }


    }
}