namespace BLAZAM.Global.Data
{
    public class AuditChangeLog
    {
        public string Field { get; set; }
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }

        public AuditChangeLog Clone()
        {
            return new AuditChangeLog() { Field=Field, OldValue=OldValue,NewValue=NewValue };
        }
    }
}