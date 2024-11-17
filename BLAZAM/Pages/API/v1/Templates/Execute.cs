using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLAZAM.Pages.API.v1.Templates
{
    [Route("/api/v1/templates/execute")]
    public class Execute : ApiController
    {
        public Execute(IHttpContextAccessor httpContextAccessor, IActiveDirectoryContextFactory adFactory) : base(httpContextAccessor, adFactory)
        {
        }
        /// <summary>
        /// Executes a user creation template. Any required fields will need to be provided in form data        /// </summary>
        /// <param name="newUserData">
        /// A JSON formatted key value list of attribute names and values
        /// </param>
        /// <response code="200">Returns details about the user performing the test.</response>
        /// <response code="401">Unauthorized - The user is not authenticated.</response>
        /// <response code="403">Forbidden - The user does not have the required role.</response>
        [HttpPost]
        public IActionResult OnPost([FromForm] Dictionary<string, string> newUserData)
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


            var dt = DateTime.UnixEpoch.AddSeconds(lng); ;
            var str = dt.ToString();
            data.Add(title, str);
        }
    }
}
