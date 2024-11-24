using BLAZAM.Database.Context;
using BLAZAM.Helpers;
using BLAZAM.Server.Data.Services;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Services
{

    public class JwtAuthenticationEventsHandler : JwtBearerEvents
    {
        private readonly IApplicationUserStateService _userStateService; // Inject any service you need
        private readonly ICurrentUserStateService _currentUserStateService; // Inject any service you need
        private readonly IHttpContextAccessor _httpContextAccessor; // Inject any service you need
        private readonly IAppDatabaseFactory _dbFactory; // Inject any service you need

        public JwtAuthenticationEventsHandler(IHttpContextAccessor httpContextAccessor, IApplicationUserStateService userStateService, IAppDatabaseFactory dbFactory, ICurrentUserStateService currentUserStateService)
        {
            _userStateService = userStateService;
            _httpContextAccessor = httpContextAccessor;
            _currentUserStateService = currentUserStateService;
            _dbFactory = dbFactory;
        }
        /// <summary>
        /// Checks the database for the existence of the token and whether it's been revoked.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task TokenValidated(TokenValidatedContext context)
        {
            // This method is called after the JWT token has been successfully validated

            var userId = context.Principal?.Identity?.Name; // Get the user ID
                                                            // ... access other claims from context.Principal
            var userState = new ApplicationUserState(_dbFactory);
            userState.User = context.Principal;
            userState.IPAddress = context.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            _userStateService.SetUserState(userState);
            using var dbContext = await _dbFactory.CreateDbContextAsync();
            var tokenStr2 = context.SecurityToken.UnsafeToString().ToSecureString();
            var tokenHash = tokenStr2.ToPlainText().GetAppHashCode();
            var matchingToken = dbContext.ApiTokens.FirstOrDefault(t => t.TokenHash.Equals(tokenHash));
            if (matchingToken == null)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("Invalid Token");
                context.Fail("Invalid Token");
            }
            else if (matchingToken.IsRevoked || matchingToken.DeletedAt != null)
            {

                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("Token Revoked");
                context.Fail("Token Revoked");
            }
            else
            {
                matchingToken.LastUsedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();
            }
            //return Task.CompletedTask;
        }


    }

}
