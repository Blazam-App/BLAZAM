using BLAZAM.Database.Context;
using BLAZAM.Services.Background;
using BLAZAM.Services.Events;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Services.Audit
{
    public class RulesAuditLogger : BaseAuditLogger
    {
        public RulesAuditLogger(IAppDatabaseFactory factory, IApplicationUserState ruleUserState) : base(factory, null)
        {
            System = new SystemAudit(factory);
            User = new UserAudit(factory) { CurrentUser = ruleUserState };
            Group = new GroupAudit(factory) { CurrentUser = ruleUserState };
            Computer = new ComputerAudit(factory) { CurrentUser = ruleUserState };
            OU = new OUAudit(factory) { CurrentUser = ruleUserState };
            Printer = new PrinterAudit(factory) { CurrentUser = ruleUserState };
            BitLocker = new BitLockerAudit(factory) { CurrentUser = ruleUserState };
            Email = new EmailAudit(factory);
            ApplicationEvents.DirectoryEntryChanged.Delegate += TriggerDirectoryEntryChangedEvent;

        }

        protected override void TriggerDirectoryEntryChangedEvent(object? sender, DirectoryEntryChangedArgs args)
        {
            if (sender != null && sender is RulesProcessor)
            {
                System = new SystemAudit(_factory);
                User = new UserAudit(_factory) { CurrentUser = args.Actor };
                Group = new GroupAudit(_factory) { CurrentUser = args.Actor };
                Computer = new ComputerAudit(_factory) { CurrentUser = args.Actor };
                OU = new OUAudit(_factory) { CurrentUser = args.Actor };
                Printer = new PrinterAudit(_factory) { CurrentUser = args.Actor };
                BitLocker = new BitLockerAudit(_factory) { CurrentUser = args.Actor };
                Email = new EmailAudit(_factory);
                base.TriggerDirectoryEntryChangedEvent(sender, args);
            }
        }

    }
}