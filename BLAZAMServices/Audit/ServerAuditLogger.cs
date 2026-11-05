using BLAZAM.Services.Background;
using BLAZAM.Session;

namespace BLAZAM.Services.Audit
{
    public class ServerAuditLogger : BaseAuditLogger
    {
        public ServerAuditLogger(IAppDatabaseFactory factory) : base(factory, null)
        {
        }

        protected override void TriggerDirectoryEntryChangedEvent(object? sender, DirectoryEntryChangedEventArgs args)
        {
            if (args.Actor.Id == 0)
            {
                base.TriggerDirectoryEntryChangedEvent(sender, args);
            }
        }

    }
}