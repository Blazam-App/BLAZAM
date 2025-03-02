using BLAZAM.Database.Context;
using BLAZAM.Session;
using BLAZAM.Session.Interfaces;
using Microsoft.JSInterop;

namespace BLAZAM.Services.Audit
{
    public class CommonAudit : BaseAudit
    {
        protected IApplicationUserStateService? UserStateService { get; private set; }
        /// <summary>
        /// The CurrentUser being auditted
        /// </summary>
        /// <remarks>
        /// The default value is the current web user from the <see cref="IApplicationUserStateService"/>
        /// </remarks>
        protected IApplicationUserState? CurrentUser { get; set; }


        public CommonAudit(IAppDatabaseFactory factory,  IApplicationUserStateService? userStateService=null, IJSRuntime? jSRuntime=null) : base(factory, jSRuntime)
        {
            UserStateService = userStateService;
            if (UserStateService != null)
            {
                CurrentUser = UserStateService.CurrentUserState;
            }
            else
            {
                CurrentUser = new SystemUserState(factory);
            }
        }
    }
}