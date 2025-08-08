using BLAZAM.Database.Context;
using System.Security.Claims;

namespace BLAZAM.Session
{
    public class RulesUserState : ApplicationUserState
    {
        public RulesUserState(IAppDatabaseFactory factory, string? ruleName = null) : base(factory)
        {
            var username = "Rules" + " [" + ruleName + "]";
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.Name, username));
            this.User = new ClaimsPrincipal();
            this.User.AddIdentity(identity);
        }
    }
}
