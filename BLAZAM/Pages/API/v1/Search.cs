using System.Text.RegularExpressions;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Searchers;
using BLAZAM.Services.Audit;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BLAZAM.Pages.API.v1
{
    /// <summary>
    /// Searches Active Directory.
    /// </summary>
    public partial class Search : ApiControllerBase
    {
        public Search(IApplicationUserStateService applicationUserStateService, WebUserAuditLogger audit, IUserDatabaseFactory appDatabaseFactory, IHttpContextAccessor httpContextAccessor, IActiveDirectoryContextFactory adFactory) : base(applicationUserStateService, audit, appDatabaseFactory, httpContextAccessor, adFactory)
        {
        }



        /// <summary>
        /// Run a general search term query against all AD object types.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/v1/search?query=fragment
        ///
        /// </remarks>
        /// <param name="query">The string fragment to search for</param>
        /// <response code="200">Returns a list of matching Active Directory objects.</response>
        /// <response code="401">Unauthorized - The user is not authenticated.</response>
        /// <response code="403">Forbidden - The user does not have the required role.</response>
        [HttpGet]
        public IActionResult OnGet([FromQuery] string query)
        {

            // Validate the query parameter: allow alphanumeric, space, hyphen, underscore, and period, 1-100 chars
            if (string.IsNullOrWhiteSpace(query) || query.Length > 100 || !ValidSearchCharactersRegex().IsMatch(query))
            {
                return BadRequest("Invalid query parameter.");
            }

            ADSearch search = new(Directory)
            {
                GeneralSearchTerm = query
            };
            var data = search.Search();
            var data2 = data.Where(de => de.CanRead).ToList();
            var data3 = data2.ToList();
            return FormatData(data3);
        }

        [GeneratedRegex(@"^[\w\s\-.]+$")]
        private static partial Regex ValidSearchCharactersRegex();
    }
}
