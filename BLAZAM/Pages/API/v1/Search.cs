using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Searchers;
using BLAZAM.Database.Context;
using BLAZAM.Services.Audit;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace BLAZAM.Pages.API.v1
{
    /// <summary>
    /// Searches Active Directory.
    /// </summary>
    [Produces("application/json")]
    public class Search : ApiController
    {
        public Search(IApplicationUserStateService applicationUserStateService, AuditLogger audit, IUserDatabaseFactory appDatabaseFactory, IHttpContextAccessor httpContextAccessor, IActiveDirectoryContextFactory adFactory) : base(applicationUserStateService, audit, appDatabaseFactory, httpContextAccessor, adFactory)
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
            // restrict the username and password to letters only
            if (!Regex.IsMatch(query, "^[a-zA-Z]+$"))
            {
                return BadRequest();
            }
            ADSearch search = new ADSearch(Directory);
            search.GeneralSearchTerm = query;
            var data = search.Search();
            var data2 = data.Where(de => de.CanRead).ToList();
            var data3 = data2.Select(de => de.CanonicalName).ToList();
            return FormatData(data3);
        }
    }
}
