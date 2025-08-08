using BLAZAM.Database.Context;
using System.Security.Claims;

namespace BLAZAM.Session
{
    public class SystemUserState : ApplicationUserState
    {
        public SystemUserState(IAppDatabaseFactory factory) : base(factory)
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.Name, "Blazam"));
            this.User = new System.Security.Claims.ClaimsPrincipal();
            this.User.AddIdentity(identity);
        }
    }
}
