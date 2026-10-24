
using BLAZAM.Session.Interfaces;
using Microsoft.JSInterop;

namespace BLAZAM.Services.Audit
{
    public class WebUserAuditLogger : BaseAuditLogger
    {
        private readonly IApplicationUserStateService _userStateService;
        private bool _customUserState;

        public WebUserAuditLogger(IAppDatabaseFactory factory, IApplicationUserStateService userStateService, IJSRuntime jSRuntime) : base(factory, userStateService.CurrentUserState.AuditActor)
        {

            _userStateService = userStateService;
            System = new SystemAudit(factory, jSRuntime);
            User = new UserAudit(factory, userStateService.CurrentUserState.AuditActor, jSRuntime);
            Group = new GroupAudit(factory, userStateService.CurrentUserState.AuditActor, jSRuntime);
            Computer = new ComputerAudit(factory, userStateService.CurrentUserState.AuditActor, jSRuntime);
            OU = new OUAudit(factory, userStateService.CurrentUserState.AuditActor, jSRuntime);
            Printer = new PrinterAudit(factory, userStateService.CurrentUserState.AuditActor, jSRuntime);
            Logon = new LogonAudit(factory, userStateService.CurrentUserState.AuditActor, jSRuntime);
            BitLocker = new BitLockerAudit(factory, userStateService.CurrentUserState.AuditActor    , jSRuntime);
        }
        public WebUserAuditLogger(IAppDatabaseFactory factory, IApplicationUserState userState, IJSRuntime jSRuntime) : base(factory, null)
        {
            _customUserState = true;
            System = new SystemAudit(factory, jSRuntime);
            User = new UserAudit(factory, userState.AuditActor, jSRuntime);
            Group = new GroupAudit(factory, userState.AuditActor, jSRuntime);
            Computer = new ComputerAudit(factory, userState.AuditActor, jSRuntime);
            OU = new OUAudit(factory, userState.AuditActor, jSRuntime);
            Printer = new PrinterAudit(factory, userState.AuditActor, jSRuntime);
            Logon = new LogonAudit(factory, userState.AuditActor, jSRuntime);
            BitLocker = new BitLockerAudit(factory, userState.AuditActor, jSRuntime);
        }

        protected override void TriggerDirectoryEntryChangedEvent(object? sender, DirectoryEntryChangedEventArgs args)
        {
            if (_customUserState || _userStateService.CurrentUserState?.SessionId.Equals(args.Actor.SessionId) == true)
            {
                base.TriggerDirectoryEntryChangedEvent(sender, args);
            }
        }

    }
}