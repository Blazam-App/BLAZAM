using BLAZAM.Database.Context;
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

            public JwtAuthenticationEventsHandler(IHttpContextAccessor httpContextAccessor,IApplicationUserStateService userStateService,IAppDatabaseFactory dbFactory,ICurrentUserStateService currentUserStateService)
            {
                _userStateService = userStateService;
            _httpContextAccessor = httpContextAccessor;
            _currentUserStateService = currentUserStateService;
            _dbFactory = dbFactory;
            }

            public override Task TokenValidated(TokenValidatedContext context)
            {
                // This method is called after the JWT token has been successfully validated

                // 1. Access user information
                var userId = context.Principal.Identity.Name; // Get the user ID
                                                              // ... access other claims from context.Principal
            var userState = new ApplicationUserState(_dbFactory);
            userState.User = context.Principal;
            userState.IPAddress = context.HttpContext?.Connection?.RemoteIpAddress?.ToString();
               _userStateService.SetUserState(userState); 

            return Task.CompletedTask;
            }

            // Other event handlers (optional)
         
        }
    
}
