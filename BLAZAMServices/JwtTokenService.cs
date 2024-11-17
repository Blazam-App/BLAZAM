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
        private readonly ICurrentUserStateService _currentUserStateService;
        private readonly ApplicationInfo _applicationInfo;

        public JwtTokenService(ApplicationInfo applicationInfo, ICurrentUserStateService currentUserStateService)

        {
            _currentUserStateService = currentUserStateService;
            _applicationInfo = applicationInfo;
        }

        public string GenerateJwtToken(string userName, string userGuid)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new Dictionary<string, object>
            {
                { ClaimTypes.Sid, userGuid }
            };
            // Get key from config
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = _currentUserStateService.State.User.Claims.FirstOrDefault()?.Subject,
                //Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, userName) }),
                Claims = claims,
                IssuedAt = DateTime.UtcNow,
                Issuer = DatabaseCache.ApplicationSettings.AppName,
                Expires = DateTime.UtcNow.AddDays(365), // Set expiration
                SigningCredentials = new SigningCredentials(_applicationInfo.TokenKey, SecurityAlgorithms.HmacSha256Signature)
            };
            var autheenticated = tokenDescriptor.Subject.IsAuthenticated;
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
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
                    IssuerSigningKey = _applicationInfo.TokenKey,
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
