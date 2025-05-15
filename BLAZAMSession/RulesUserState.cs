using BLAZAM.Database.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Session
{
    public class RulesUserState : ApplicationUserState
    {
        public RulesUserState(IAppDatabaseFactory factory,string? ruleName=null) : base(factory)
        {
            var username = "Rules" + " [" + ruleName + "]";
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.Name, username));
            this.User = new ClaimsPrincipal();
            this.User.AddIdentity(identity);
        }
    }
}
