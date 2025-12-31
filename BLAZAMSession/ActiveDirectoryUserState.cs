using System.Security.Claims;

namespace BLAZAM.Session
{
    public class ActiveDirectoryUserState : ApplicationUserState
    {
        public ActiveDirectoryUserState(IAppDatabaseFactory factory) : base(factory)
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.Name, "Active Directory"));
            identity.AddClaim(new Claim(ClaimTypes.WindowsAccountName, "Active Directory"));
            this.User = new System.Security.Claims.ClaimsPrincipal();
            this.User.AddIdentity(identity);
        }
    }
}
