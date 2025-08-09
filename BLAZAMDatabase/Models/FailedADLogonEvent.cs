namespace BLAZAM.Database.Models
{
    public class FailedADLogonEvent : AppDbSetBase
    {
        public string? WorkstationName { get; set; }
        public string? WorkstationIp { get; set; }
        public DateTime? Timestamp { get; set; }
        public byte[] Sid { get; set; }
    }
}
