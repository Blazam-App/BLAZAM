using BLAZAM.Database.Context;
using BLAZAM.Logger;
using BLAZAM.Session.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace BLAZAM.Services
{
    /// <summary>
    /// Service responsible for generating and decoding JWT (JSON Web Tokens) for API authentication.
    /// </summary>
    public class JwtTokenService
    {
        private readonly ICurrentUserStateService _currentUserStateService;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenService"/> class.
        /// </summary>
        /// <param name="currentUserStateService">Service to access the current user's state for generating token claims.</param>
        /// <exception cref="ArgumentNullException">Thrown if currentUserStateService is null.</exception>
        public JwtTokenService(ICurrentUserStateService currentUserStateService)
        {
            ArgumentNullException.ThrowIfNull(currentUserStateService);

            _currentUserStateService = currentUserStateService;
        }

        /// <summary>
        /// Generates a new JWT for the current user, typically for API access. The token is configured with claims based on the user's identity and has a defined lifetime.
        /// </summary>
        /// <param name="lifetime">The timespan for which the token will be valid. Defaults to 30 days if null.</param>
        /// <returns>A string representation of the generated JWT.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the current user state or user identity is unavailable.</exception>
        public string GenerateJwtToken(TimeSpan? lifetime = null)
        {
            if (_currentUserStateService.State == null)
            {
                Loggers.SystemLogger.Error("JwtTokenService.GenerateJwtToken: _currentUserStateService.State is null. Cannot generate token.");
                throw new InvalidOperationException("_currentUserStateService.State is null. Cannot generate token.");
            }
            var currentUser = _currentUserStateService.State;

            if (currentUser.User == null)
            {
                Loggers.SystemLogger.Error("JwtTokenService.GenerateJwtToken: currentUser.User is null for UserGUID {UserGUID}. Cannot generate token.", currentUser.Preferences?.UserGUID ?? "Unknown");
                throw new InvalidOperationException("currentUser.User is null. Cannot generate token.");
            }

            if (lifetime == null) { lifetime = TimeSpan.FromDays(30); }
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new Dictionary<string, object>
            {
                { ClaimTypes.Sid, currentUser.Preferences.UserGUID} // UserGUID should be available if State and User are.
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = currentUser.User.Claims.FirstOrDefault()?.Subject,
                Claims = claims,
                Audience = ApplicationInfo.installationId.ToString(),
                IssuedAt = DateTime.UtcNow,
                Issuer = DatabaseCache.ApplicationSettings?.AppName, // Assumes AppSettings and AppName are available
                Expires = (DateTime.UtcNow + lifetime.Value),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encryption.Instance.APITokenKey), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            return jwtToken;
        }

        /// <summary>
        /// Decodes and validates a JWT string.
        /// </summary>
        /// <param name="token">The JWT string to decode.</param>
        /// <returns>A <see cref="ClaimsPrincipal"/> representing the identity in the token, or null if validation fails.</returns>
        public ClaimsPrincipal DecodeJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encryption.Instance.APITokenKey),
                    ValidateIssuer = true,
                    ValidIssuer = DatabaseCache.ApplicationSettings?.AppName,
                    ValidateAudience = true,
                    ValidAudience = ApplicationInfo.installationId.ToString(),  
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                return new ClaimsPrincipal(new ClaimsIdentity(jwtToken.Claims));
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error("Error validating token: {ErrorMessage} Token: {TokenString}", ex.Message, token);
                return null;
            }
        }
    }
}
