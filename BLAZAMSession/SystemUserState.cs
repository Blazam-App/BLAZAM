using BLAZAM.Database.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Session
{
    public class SystemUserState : ApplicationUserState
    {
        public SystemUserState(IAppDatabaseFactory factory) : base(factory)
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.Name, "System"));
            this.User = new System.Security.Claims.ClaimsPrincipal();
            this.User.AddIdentity(identity);
        }
    }
}
