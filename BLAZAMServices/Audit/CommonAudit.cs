using BLAZAM.Database.Context;
using BLAZAM.Session;
using BLAZAM.Session.Interfaces;
using Microsoft.JSInterop;

namespace BLAZAM.Services.Audit
{
    public class CommonAudit : BaseAudit
    {
        protected IApplicationUserState? UserState { get; set; }
        /// <summary>
        /// The CurrentUser being auditted
        /// </summary>
        /// <remarks>
        /// The default value is the current web user from the <see cref="IApplicationUserStateService"/>
        /// </remarks>
        public IApplicationUserState? CurrentUser { get; set; }


        public CommonAudit(IAppDatabaseFactory factory, IApplicationUserState? userState = null, IJSRuntime? jSRuntime = null) : base(factory, jSRuntime)
        {

            if (userState != null)
            {
                UserState = userState;
            }
            else
            {
                UserState = new SystemUserState(factory);
            }
        }
    }
}