using BLAZAM.Services.Events;
using BLAZAM.Session;

namespace BLAZAM.Services.Audit
{
    public class ServerAuditLogger : BaseAuditLogger
    {
        public ServerAuditLogger(IAppDatabaseFactory factory) : base(factory, null)
        {

            System = new SystemAudit(factory);
            User = new UserAudit(factory);
            Group = new GroupAudit(factory);
            Computer = new ComputerAudit(factory);
            OU = new OUAudit(factory);
            Printer = new PrinterAudit(factory);
            BitLocker = new BitLockerAudit(factory);
            Email = new EmailAudit(factory);
        }

        protected override void TriggerDirectoryEntryChangedEvent(object? sender, DirectoryEntryChangedArgs args)
        {
            if (new SystemUserState(_factory).Equals(args.Actor) == true)
            {
                base.TriggerDirectoryEntryChangedEvent(sender, args);
            }
        }

    }
}