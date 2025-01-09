using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Services;
using BLAZAM.Database.Context;
using BLAZAM.Logger;
using BLAZAM.Server.Data.Services;
using BLAZAM.Session.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Services
{
    public class JwtTokenService
    {
        private readonly IEncryptionService _encryptionService;
        private readonly ICurrentUserStateService _currentUserStateService;
        private readonly ApplicationInfo _applicationInfo;

        public JwtTokenService(IEncryptionService encryptionService, ApplicationInfo applicationInfo, ICurrentUserStateService currentUserStateService)

        {
            _encryptionService = encryptionService;
            _currentUserStateService = currentUserStateService;
            _applicationInfo = applicationInfo;
        }
        /// <summary>
        /// Generates a new <see cref="JwtSecurityToken"/> for the <see cref="ICurrentUserStateService"/> user and places
        /// it into the database
        /// </summary>
        /// <param name="lifetime">The amount of time the token should be allowed to be used. Defaults to 365 days.</param>
        /// <returns>The newly generated <see cref="JwtSecurityToken"/></returns>
        public string GenerateJwtToken(TimeSpan? lifetime = null)
        {
            if (lifetime == null) { lifetime = TimeSpan.FromDays(365); }
            var tokenHandler = new JwtSecurityTokenHandler();

            var currentUser = _currentUserStateService.State;
            var claims = new Dictionary<string, object>
            {
                { ClaimTypes.Sid, currentUser.Preferences.UserGUID}
            };
            // Get key from config
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = currentUser.User.Claims.FirstOrDefault()?.Subject,
                Claims = claims,
                IssuedAt = DateTime.UtcNow,
                Issuer = DatabaseCache.ApplicationSettings.AppName,
                Expires = (DateTime.UtcNow + lifetime.Value), // Set expiration
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encryption.Instance.APITokenKey), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);



            return jwtToken;
        }

        public ClaimsPrincipal DecodeJwtToken(string
     token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();


            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encryption.Instance.APITokenKey),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero // Set clock skew to zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                return new ClaimsPrincipal(new ClaimsIdentity(jwtToken.Claims));

            }
            catch (Exception ex)
            {
                // Handle token validation errors (e.g., log the error)
                Loggers.SystemLogger.Error($"Error validating token: {ex.Message}");
                return null;
            }
        }
    }
}
