using BLAZAM.Services.Background;
using BLAZAM.Services.Events;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Services.Audit
{
    public class RulesAuditLogger : BaseAuditLogger
    {
        public RulesAuditLogger(IAppDatabaseFactory factory) : base(factory, null)
        {
            
            ApplicationEvents.DirectoryEntryEvent.Delegate += TriggerDirectoryEntryChangedEvent;

        }

        protected override void TriggerDirectoryEntryChangedEvent(object? sender, DirectoryEntryChangedArgs args)
        {
            if (sender != null && sender is RulesProcessor)
            {
                System = new SystemAudit(_factory);
                User = new UserAudit(_factory, args.Actor);
                Group = new GroupAudit(_factory, args.Actor);
                Computer = new ComputerAudit(_factory, args.Actor);
                OU = new OUAudit(_factory, args.Actor);
                Printer = new PrinterAudit(_factory, args.Actor);
                BitLocker = new BitLockerAudit(_factory, args.Actor);
                Email = new EmailAudit(_factory);
                base.TriggerDirectoryEntryChangedEvent(sender, args);
            }
        }

    }
}