using BLAZAM.Helpers;

namespace BLAZAM.Global.Data
{
    public class AuditLogActor

    {
        public string Actor { get; set; }
        public string? Impersonator { get; set; }
        public string? IPAddress { get; set; }
        public int Id { get; set; }
        public Guid SessionId { get; set; }

        public string AuditUsername { get => Impersonator.IsNullOrEmpty() ? Actor : $"{Actor} (Impersonated by {Impersonator})"; }
    }
}
