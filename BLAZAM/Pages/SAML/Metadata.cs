using ITfoxtec.Identity.Saml2.Schemas.Metadata;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;
using ITfoxtec.Identity.Saml2.MvcCore;
using Microsoft.Extensions.Options;

namespace BLAZAM.Server.Pages.SAML
{
    [AllowAnonymous]
    [Route("Metadata")]
    public class Metadata : Controller
    {
        private readonly Saml2Configuration config;

        public Metadata(IOptions<Saml2Configuration> configAccessor)
        {
            config = configAccessor.Value;
        }

        public IActionResult Index()
        {
            var defaultSite = new Uri($"{Request.Scheme}://{Request.Host.ToUriComponent()}/");

            var entityDescriptor = new EntityDescriptor(config);
            entityDescriptor.ValidUntil = 365;
            entityDescriptor.SPSsoDescriptor = new SPSsoDescriptor
            {
                WantAssertionsSigned = true,
                SigningCertificates = new X509Certificate2[]
                {
                    config.SigningCertificate
                },
                //EncryptionCertificates = new X509Certificate2[]
                //{
                //    config.DecryptionCertificate
                //},
                SingleLogoutServices = new SingleLogoutService[]
                {
                    new SingleLogoutService { Binding = ProtocolBindings.HttpPost, Location = new Uri(defaultSite, "sso/SingleLogout"), ResponseLocation = new Uri(defaultSite, "Auth/LoggedOut") }
                },
                NameIDFormats = new Uri[] { NameIdentifierFormats.X509SubjectName },
                AssertionConsumerServices = new AssertionConsumerService[]
                {
                    new AssertionConsumerService { Binding = ProtocolBindings.HttpPost, Location = new Uri(defaultSite, "sso/AssertionConsumerService") },
                },
                AttributeConsumingServices = new AttributeConsumingService[]
                {
                    new AttributeConsumingService { ServiceName = new ServiceName("Blazam", "en"), RequestedAttributes = CreateRequestedAttributes() }
                },
            };
            /*
             entityDescriptor.ContactPersons = new[] {
                 new ContactPerson(ContactTypes.Administrative)
                 {
                     Company = "Some Company",
                     GivenName = "Some Given Name",
                     SurName = "Some Sur Name",
                     EmailAddress = "some@some-domain.com",
                     TelephoneNumber = "11111111",
                 },
                 new ContactPerson(ContactTypes.Technical)
                 {
                     Company = "Some Company",
                     GivenName = "Some tech Given Name",
                     SurName = "Some tech Sur Name",
                     EmailAddress = "sometech@some-domain.com",
                     TelephoneNumber = "22222222",
                 }
             };
            */
            return new Saml2Metadata(entityDescriptor).CreateMetadata().ToActionResult();
        }

        private IEnumerable<RequestedAttribute> CreateRequestedAttributes()
        {
            yield return new RequestedAttribute("urn:oid:2.5.4.4");
            yield return new RequestedAttribute("urn:oid:2.5.4.3", false);
            yield return new RequestedAttribute("urn:xxx", "test-value");
            yield return new RequestedAttribute("urn:yyy", "123") { AttributeValueType = "xs:integer" };
        }
    }
}
