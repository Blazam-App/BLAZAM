using BLAZAM.Common.Data.Services;
using BLAZAM.Logger;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
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

        public JwtTokenService(IEncryptionService encryptionService)

        {
            _encryptionService = encryptionService;
        }

        public string GenerateJwtToken(string userName,string userGuid)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = _encryptionService.Key;
            var claims = new Dictionary<string, object>
            {
                { ClaimTypes.Sid, userGuid }
            };
            // Get key from config
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, userName) }),
                 Claims  = claims,
                Expires     = DateTime.UtcNow.AddDays(7), // Set expiration
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal DecodeJwtToken(string
     token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = _encryptionService.Key; // Get key from config

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer  = false,
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
