using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using System.Security.Claims;

namespace BLAZAM.Pages.API.v1
{
    [Authorize]
    public class TestModel : PageModel
    {

        public TestModel(IHttpContextAccessor httpContextAccessor, ICurrentUserStateService currentUserStateService)
        {
            var user = httpContextAccessor.HttpContext.User;
            CurrentUser = user;
        }

        public ClaimsPrincipal CurrentUser { get; }

        [HttpGet]
        public IActionResult OnGet()
        {
            var data = new Dictionary<string,string?>();
            data.Add("Username", CurrentUser.Identity.Name);
            return new JsonResult(data);
        }
    }
}
