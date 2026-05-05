using BLAZAM.Services.Events;
using BLAZAM.Session.Interfaces;
using Microsoft.JSInterop;

namespace BLAZAM.Services.Audit
{
    public class WebUserAuditLogger : BaseAuditLogger
    {
        private readonly IApplicationUserStateService _userStateService;
        private bool _customUserState;

        public WebUserAuditLogger(IAppDatabaseFactory factory, IApplicationUserStateService userStateService, IJSRuntime jSRuntime) : base(factory, userStateService.CurrentUserState)
        {

            _userStateService = userStateService;
            System = new SystemAudit(factory, jSRuntime);
            User = new UserAudit(factory, userStateService.CurrentUserState, jSRuntime);
            Group = new GroupAudit(factory, userStateService.CurrentUserState, jSRuntime);
            Computer = new ComputerAudit(factory, userStateService.CurrentUserState, jSRuntime);
            OU = new OUAudit(factory, userStateService.CurrentUserState, jSRuntime);
            Printer = new PrinterAudit(factory, userStateService.CurrentUserState, jSRuntime);
            Logon = new LogonAudit(factory, userStateService.CurrentUserState, jSRuntime);
            BitLocker = new BitLockerAudit(factory, userStateService.CurrentUserState, jSRuntime);
        }
        public WebUserAuditLogger(IAppDatabaseFactory factory, IApplicationUserState userState, IJSRuntime jSRuntime) : base(factory, null)
        {
            _customUserState = true;
            System = new SystemAudit(factory, jSRuntime);
            User = new UserAudit(factory, userState, jSRuntime);
            Group = new GroupAudit(factory, userState, jSRuntime);
            Computer = new ComputerAudit(factory, userState, jSRuntime);
            OU = new OUAudit(factory, userState, jSRuntime);
            Printer = new PrinterAudit(factory, userState, jSRuntime);
            Logon = new LogonAudit(factory, userState, jSRuntime);
            BitLocker = new BitLockerAudit(factory, userState, jSRuntime);
        }

        protected override void TriggerDirectoryEntryChangedEvent(object? sender, DirectoryEntryChangedArgs args)
        {
            if (_customUserState || _userStateService.CurrentUserState?.Equals(args.Actor) == true)
            {
                base.TriggerDirectoryEntryChangedEvent(sender, args);
            }
        }

    }
}