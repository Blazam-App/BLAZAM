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
        public RulesAuditLogger(IAppDatabaseFactory factory) : base(factory, null)
        {

            System = new SystemAudit(factory);
            User = new UserAudit(factory) { CurrentUser = new RulesUserState(factory) };
            Group = new GroupAudit(factory) { CurrentUser = new RulesUserState(factory) };
            Computer = new ComputerAudit(factory) { CurrentUser = new RulesUserState(factory) };
            OU = new OUAudit(factory) { CurrentUser = new RulesUserState(factory) };
            Printer = new PrinterAudit(factory) { CurrentUser = new RulesUserState(factory) };
            BitLocker = new BitLockerAudit(factory) { CurrentUser = new RulesUserState(factory) };
            Email=new EmailAudit(factory);
        }

        protected override void ProcessDirectoryEntryChangedEvent(object? sender, DirectoryEntryChangedArgs args)
        {
            //Don't trigger audit on user invoked events
            //if (new SystemUserState(_factory).Equals(args.Actor) == true)
            //{
            //    base.ProcessDirectoryEntryChangedEvent(sender,args);
            //}
        }

    }
}