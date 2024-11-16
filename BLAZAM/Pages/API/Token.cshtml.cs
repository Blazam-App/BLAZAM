using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Database;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models.User;
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
        private readonly IAppDatabaseFactory _factory;

        public JwtSecurityTokenHandler JwtTokenHandler { get; private set; }
        public string Token { get; private set; }


        public TokenModel(IAppDatabaseFactory factory)
        {
            _factory = factory;
        }

        public JsonResult OnGet()
        {
            JwtTokenHandler = new JwtSecurityTokenHandler();
            var user = this.User.Identity?.Name;
            if (string.IsNullOrEmpty(user))
            {
                throw new InvalidOperationException("Name is not specified.");
            }

            var claims = new[] { new Claim(ClaimTypes.Name, user) };
            var credentials = new SigningCredentials(ApplicationInfo.TokenKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken("Blazam", "ExampleClients", claims, expires: DateTime.Now.AddSeconds(60), signingCredentials: credentials);
            Token = JwtTokenHandler.WriteToken(token);
            using var context = _factory.CreateDbContext();
            var userSettings = context.UserSettings.Where(u => u.UserGUID == this.User.Identity.Name).FirstOrDefault();
            if (userSettings != null)
            {
                userSettings.APIToken = Token;
            }

            context.SaveChanges();
            return new JsonResult(userSettings);
        }
    }
}
