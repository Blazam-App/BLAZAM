using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Searchers;
using BLAZAM.Services.Audit;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace BLAZAM.Pages.API.v1
{
    /// <summary>
    /// Searches Active Directory.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Search"/> class.
    /// </remarks>
    /// <param name="applicationUserStateService">A service that provides access to the current application user's state.</param>
    /// <param name="audit">The logger used to record user activity for auditing purposes.</param>
    /// <param name="appDatabaseFactory">A factory for creating instances of the user database.</param>
    /// <param name="httpContextAccessor">Provides access to the current HTTP context.</param>
    /// <param name="adFactory">A factory for creating Active Directory context instances.</param>
    public partial class Search(IApplicationUserStateService applicationUserStateService, WebUserAuditLogger audit, IUserDatabaseFactory appDatabaseFactory, IHttpContextAccessor httpContextAccessor, IActiveDirectoryContextFactory adFactory) : ApiControllerBase(applicationUserStateService, audit, appDatabaseFactory, httpContextAccessor, adFactory)
    {



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
            return FormatData(data2);
        }

        [GeneratedRegex(@"^[\w\s\-.]+$")]
        private static partial Regex ValidSearchCharactersRegex();
    }
}
