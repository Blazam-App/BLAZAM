using BLAZAM.Services.Background;

namespace BLAZAM.Services.Audit
{
    public class RulesAuditLogger : BaseAuditLogger
    {
        public RulesAuditLogger(IAppDatabaseFactory factory) : base(factory, null)
        {

            ActiveDirectoryEvents.DirectoryEntryEvent.Delegate += TriggerDirectoryEntryChangedEvent;

        }

        protected override void TriggerDirectoryEntryChangedEvent(object? sender, DirectoryEntryChangedEventArgs args)
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