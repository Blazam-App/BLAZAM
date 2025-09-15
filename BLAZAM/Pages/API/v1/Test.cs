using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Services.Audit;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BLAZAM.Pages.API.v1
{
    /// <summary>
    /// A simple API endpoint to test the API configuration and return details about the authenticated user. 
    /// </summary>
    public class Test : ApiControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Test"/> class with the specified services and dependencies.
        /// </summary>
        /// <param name="applicationUserStateService">A service for managing the state of the application user.</param>
        /// <param name="audit">The logger used for auditing user actions.</param>
        /// <param name="appDatabaseFactory">A factory for creating instances of the application database.</param>
        /// <param name="httpContextAccessor">Provides access to the current HTTP context.</param>
        /// <param name="adFactory">A factory for creating Active Directory context instances.</param>
        public Test(IApplicationUserStateService applicationUserStateService, WebUserAuditLogger audit, IUserDatabaseFactory appDatabaseFactory, IHttpContextAccessor httpContextAccessor, IActiveDirectoryContextFactory adFactory) : base(applicationUserStateService, audit, appDatabaseFactory, httpContextAccessor, adFactory)
        {
        }



        /// <summary>
        /// API connection check to test the configuration.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Returns details about the user performing the test.</response>
        /// <response code="401">Unauthorized - The user is not authenticated.</response>
        /// <response code="403">Forbidden - The user does not have the required role.</response>
        [HttpGet]
        public IActionResult OnGet()
        {
            dynamic data = new Dictionary<string, object?>();
            Add(data, "Issuer", "iss");

            data.Add("Username", User.Identity?.Name);
            AddDateTime(data, "Not Before", "nbf");
            AddDateTime(data, "Issued At", "iat");
            AddDateTime(data, "Expires", "exp");
            var claims = new List<KeyValuePair<string, string?>>();
            foreach (var claim in User.Claims)
            {
                claims.Add(new(claim.Type, claim.Value));
            }
            data.Add("Claims", claims);
            return FormatData(data);
        }



        private void Add(dynamic data, string title, string key)
        {
            var raw = User.Claims.FirstOrDefault(x => x.Type == key)?.Value;

            var str = raw?.ToString();
            data.Add(title, str);
        }
        private void AddDateTime(dynamic data, string title, string key)
        {
            var raw = User.Claims.FirstOrDefault(x => x.Type == key)?.Value;
            var lng = long.Parse(raw);


            var dt = DateTime.UnixEpoch.AddSeconds(lng);
            var str = dt.ToString();
            data.Add(title, str);
        }
    }
}
