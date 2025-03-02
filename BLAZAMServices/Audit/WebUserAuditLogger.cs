using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Session.Interfaces;
using Microsoft.JSInterop;

namespace BLAZAM.Services.Audit
{
    public class WebUserAuditLogger : BaseAuditLogger
    {
        public WebUserAuditLogger(IAppDatabaseFactory factory, IApplicationUserStateService userStateService, IJSRuntime jSRuntime) : base(factory, userStateService)
        {
            System = new SystemAudit(factory, jSRuntime);
            User = new UserAudit(factory, userStateService, jSRuntime);
            Group = new GroupAudit(factory, userStateService, jSRuntime);
            Computer = new ComputerAudit(factory, userStateService, jSRuntime);
            OU = new OUAudit(factory, userStateService, jSRuntime);
            Printer = new PrinterAudit(factory, userStateService, jSRuntime);
            Logon = new LogonAudit(factory, userStateService, jSRuntime);
            BitLocker = new BitLockerAudit(factory, userStateService, jSRuntime);
        }


    }
}