using BLAZAM.Session;

namespace BLAZAM.Services.Audit
{
    public class ServerAuditLogger : BaseAuditLogger
    {
        public ServerAuditLogger(IAppDatabaseFactory factory) : base(factory, null)
        {
        }

        protected override void TriggerDirectoryEntryChangedEvent(object? sender, DirectoryEntryChangedArgs args)
        {
            if (args.Actor is SystemUserState || args.Actor is RulesUserState || args.Actor is ActiveDirectoryUserState)
            {
                base.TriggerDirectoryEntryChangedEvent(sender, args);
            }
        }

    }
}