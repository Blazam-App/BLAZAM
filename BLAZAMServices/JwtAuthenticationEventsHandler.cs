using BLAZAM.Database.Context;
using BLAZAM.Helpers;
using BLAZAM.Logger;
using BLAZAM.Session;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;

namespace BLAZAM.Services
{
    /// <summary>
    /// Handles JWT bearer events, specifically for further validating a token against the database after initial cryptographic validation.
    /// </summary>
    public class JwtAuthenticationEventsHandler : JwtBearerEvents
    {
        private readonly IApplicationUserStateService _userStateService;
        private readonly IAppDatabaseFactory _dbFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtAuthenticationEventsHandler"/> class.
        /// </summary>
        /// <param name="userStateService">Service for user state management.</param>
        /// <param name="dbFactory">Factory for creating database contexts.</param>
        /// <exception cref="ArgumentNullException">Thrown if userStateService or dbFactory is null.</exception>
        public JwtAuthenticationEventsHandler(IApplicationUserStateService userStateService, IAppDatabaseFactory dbFactory)
        {
            ArgumentNullException.ThrowIfNull(userStateService);

            ArgumentNullException.ThrowIfNull(dbFactory);

           
            _userStateService = userStateService;
            _dbFactory = dbFactory;
        }

        /// <summary>
        /// Called after the JWT token has been successfully validated by the framework. This method performs additional database checks for token validity (existence, revocation status) and updates its last used timestamp.
        /// </summary>
        /// <param name="context">The context for token validation.</param>
        public override async Task TokenValidated(TokenValidatedContext context)
        {
            Loggers.SystemLogger.Information("JwtAuthenticationEventsHandler.TokenValidated: Token validation started for user {UserId}.", context.Principal?.Identity?.Name ?? "Unknown");

            var userId = context.Principal?.Identity?.Name; // Get the user ID (already present, just for clarity)

            var userState = new ApplicationUserState(_dbFactory);
            userState.User = context.Principal;
            userState.IPAddress = context.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            _userStateService.SetUserState(userState);

            using var dbContext = await _dbFactory.CreateDbContextAsync();
            var tokenStrSecure = context.SecurityToken.UnsafeToString().ToSecureString();
            var tokenHash = tokenStrSecure.ToPlainText().GetAppHashCode(); // Ensure tokenHash is captured

            var matchingToken = dbContext.ApiTokens.FirstOrDefault(t => t.TokenHash.Equals(tokenHash));

            if (matchingToken == null)
            {
                Loggers.SystemLogger.Warning("JwtAuthenticationEventsHandler.TokenValidated: API token hash {TokenHash} not found in database for user {UserId}. Failing authentication.", tokenHash, context.Principal?.Identity?.Name ?? "Unknown");
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("Invalid Token");
                context.Fail("Invalid Token");
            }
            else if (matchingToken.IsRevoked || matchingToken.DeletedAt != null)
            {
                Loggers.SystemLogger.Warning("JwtAuthenticationEventsHandler.TokenValidated: API token for user {UserId} (DB ID: {TokenId}) is revoked or deleted. Failing authentication.", context.Principal?.Identity?.Name ?? "Unknown", matchingToken.Id);
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("Token Revoked");
                context.Fail("Token Revoked");
            }
            else
            {
                matchingToken.LastUsedAt = DateTime.UtcNow;
                try
                {
                    await dbContext.SaveChangesAsync();
                }
                catch (System.Exception ex)
                {
                    Loggers.DatabaseLogger.Error(ex, "JwtAuthenticationEventsHandler.TokenValidated: Failed to save ChangesAsync for token LastUsedAt update for user {UserId}, token ID {TokenId}.", context.Principal?.Identity?.Name ?? "Unknown", matchingToken?.Id);
                    // Decide if context.Fail() should be called here too, or if just logging is sufficient.
                    // For now, just log, as the primary validation (token existence/revocation) already passed.
                }
            }
        }
    }
}
