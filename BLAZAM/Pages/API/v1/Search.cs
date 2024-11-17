using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Searchers;
using Microsoft.AspNetCore.Mvc;

namespace BLAZAM.Pages.API.v1
{
    /// <summary>
    /// Searches Active Directory.
    /// </summary>
    public class Search : ApiController
    {
        public Search(IHttpContextAccessor httpContextAccessor, IActiveDirectoryContextFactory adFactory) : base(httpContextAccessor, adFactory)
        {
        }

        /// <summary>
        /// Run a general search term query against all AD object types.
        /// </summary>
        /// <param name="query">The string fragment to search for</param>
        /// <response code="200">Returns a list of matching Active Directory objects.</response>
        /// <response code="401">Unauthorized - The user is not authenticated.</response>
        /// <response code="403">Forbidden - The user does not have the required role.</response>
        [HttpGet]
        public IActionResult OnGet([FromQuery] string query)
        {
            ADSearch search = new ADSearch(Directory);
            search.GeneralSearchTerm = query;
            var data = search.Search();
            var data2 = data.Where(de => de.CanRead).ToList();
            var data3 = data2.Select(de => de.CanonicalName).ToList(); 
            return FormatData(data3);
        }
    }
}
