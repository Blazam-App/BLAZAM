using BLAZAM.Server.Data.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BLAZAM.Server.Pages.API
{
    public class ValidateUpdateTokenModel : PageModel
    {

        public IActionResult OnGet(string updateToken)
        {
            if (updateToken.Equals(AdminTokenService.Token.Guid.ToString()))
                return new OkResult();

            return new UnauthorizedResult();
        }
    }
}
