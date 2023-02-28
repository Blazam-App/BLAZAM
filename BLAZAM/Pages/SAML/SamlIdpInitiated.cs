﻿using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2.Util;
using ITfoxtec.Identity.Saml2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.Security.Claims;
using ITfoxtec.Identity.Saml2.MvcCore;

namespace BLAZAM.Server.Pages.SAML
{
    public class SamlIdpInitiated : Controller
    {
        [AllowAnonymous]
        [Route("IdPInitiated")]
        public class IdPInitiatedController : Controller
        {
            public IActionResult Initiate()
            {
                var serviceProviderRealm = "https://some-domain.com/some-service-provider";

                var binding = new Saml2PostBinding();
                binding.RelayState = $"RPID={Uri.EscapeDataString(serviceProviderRealm)}";
                var config = new Saml2Configuration();

                config.Issuer = "http://some-domain.com/this-application";
                config.SingleSignOnDestination = new Uri("https://test-adfs.itfoxtec.com/adfs/ls/");
                // config.SigningCertificate = CertificateUtil.Load(Startup.AppEnvironment.MapToPhysicalFilePath("itfoxtec.identity.saml2.testwebappcore_Certificate.pfx"), "!QAZ2wsx");
                config.SignatureAlgorithm = Saml2SecurityAlgorithms.RsaSha256Signature;

                var appliesToAddress = "https://test-adfs.itfoxtec.com/adfs/services/trust";

                var response = new Saml2AuthnResponse(config);
                response.Status = Saml2StatusCodes.Success;

                var claimsIdentity = new ClaimsIdentity(CreateClaims());
                response.NameId = new Saml2NameIdentifier(claimsIdentity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).Single(), NameIdentifierFormats.Persistent);
                response.ClaimsIdentity = claimsIdentity;
                var token = response.CreateSecurityToken(appliesToAddress);

                return binding.Bind(response).ToActionResult();
            }

            private IEnumerable<Claim> CreateClaims()
            {
                yield return new Claim(ClaimTypes.NameIdentifier, "some-user-identity");
                yield return new Claim(ClaimTypes.Email, "some-user@domain.com");
            }
        }
    }
}
