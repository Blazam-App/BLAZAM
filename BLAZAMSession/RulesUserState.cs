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
        public RulesUserState(IAppDatabaseFactory factory) : base(factory)
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.Name, "Rules"));
            this.User = new System.Security.Claims.ClaimsPrincipal();
            this.User.AddIdentity(identity);
        }
    }
}
