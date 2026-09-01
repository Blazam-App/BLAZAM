using BLAZAM.Localization;
using System.Security.Claims;

namespace BLAZAM.Session
{
    public class RulesUserState : ApplicationUserState
    {
        public RulesUserState(IAppDatabaseFactory factory, string? ruleName = null) : base(factory)
        {
            var username = AppLocalization.Rule.ToString() + " [" + ruleName + "]";
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.Name, username));
            identity.AddClaim(new Claim(ClaimTypes.WindowsAccountName, username));
            this.User = new ClaimsPrincipal();
            this.User.AddIdentity(identity);
            this.PermissionDelegates.Add(new()
            {
                DelegateName = "Rules",
                IsSuperAdmin = true,
                DelegateSid = [1],
                PermissionsMaps = [],
                Id = 0
            });
        }
    }
}
