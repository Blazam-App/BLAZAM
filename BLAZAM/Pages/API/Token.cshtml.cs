using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Models.Database;
using BLAZAM.Common.Models.Database.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BLAZAM.Server.Pages.API
{
    public class TokenModel : PageModel
    {
        public JwtSecurityTokenHandler JwtTokenHandler { get; private set; }
        public string Token { get; private set; }
        public  IDatabaseContext Context { get; private set; }

      
        public TokenModel(IDatabaseContext context)
        {
            Context = context;
        }

        public JsonResult OnGet()
        {
            JwtTokenHandler = new JwtSecurityTokenHandler();
            var user = this.User.Identity.Name;
            if (string.IsNullOrEmpty(user))
            {
                throw new InvalidOperationException("Name is not specified.");
            }

            var claims = new[] { new Claim(ClaimTypes.Name, user) };
            var credentials = new SigningCredentials(Program.TokenKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken("ExampleServer", "ExampleClients", claims, expires: DateTime.Now.AddSeconds(60), signingCredentials: credentials);
            Token = JwtTokenHandler.WriteToken(token);
            var userSettings = Context.UserSettings.Where(u => u.UserGUID == this.User.Identity.Name).FirstOrDefault();
            if(userSettings != null)
            {
                userSettings.APIToken = Token;
            }
            else
            {
                userSettings = new AppUser
                {
                    UserGUID = User.Identity.Name,
                    APIToken = Token
                };
                    Context.UserSettings.Add(userSettings);
                
            }
            Context.SaveChanges();
            return new JsonResult(userSettings);
        }
    }
}
