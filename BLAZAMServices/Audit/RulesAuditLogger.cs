using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Services.Events;
using BLAZAM.Session;
using BLAZAM.Session.Interfaces;
using Microsoft.JSInterop;

namespace BLAZAM.Services.Audit
{
    public class RulesAuditLogger : BaseAuditLogger
    {
        public RulesAuditLogger(IAppDatabaseFactory factory,IApplicationUserState ruleUserState) : base(factory, null)
        {
            System = new SystemAudit(factory);
            User = new UserAudit(factory) { CurrentUser = ruleUserState };
            Group = new GroupAudit(factory) { CurrentUser = ruleUserState };
            Computer = new ComputerAudit(factory) { CurrentUser = ruleUserState };
            OU = new OUAudit(factory) { CurrentUser = ruleUserState };
            Printer = new PrinterAudit(factory) { CurrentUser = ruleUserState };
            BitLocker = new BitLockerAudit(factory) { CurrentUser = ruleUserState };
            Email=new EmailAudit(factory);
        }
      
        protected override void TriggerDirectoryEntryChangedEvent(object? sender, DirectoryEntryChangedArgs args)
        {
            //Don't trigger audit on user invoked events
            //if (new SystemUserState(_factory).Equals(args.Actor) == true)
            //{
            //    base.ProcessDirectoryEntryChangedEvent(sender,args);
            //}
        }

    }
}