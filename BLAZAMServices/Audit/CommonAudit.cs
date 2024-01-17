using BLAZAM.Database.Context;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Services.Audit
{
    public class CommonAudit : BaseAudit
    {
        protected IApplicationUserStateService UserStateService { get; private set; }
        protected IApplicationUserState? CurrentUser { get; set; }
        public CommonAudit(IAppDatabaseFactory factory, IApplicationUserStateService userStateService) : base(factory)
        {
            UserStateService = userStateService;
            CurrentUser = UserStateService.CurrentUserState;

        }
    }
}