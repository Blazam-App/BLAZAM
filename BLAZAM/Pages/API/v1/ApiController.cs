using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BLAZAM.Pages.API.v1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Login)]
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    public class ApiController : Controller
    {
        private DateTime _startTime = DateTime.Now;
        protected Dictionary<string, object?> ResponseData = new();

        public ApiController(IHttpContextAccessor httpContextAccessor, IActiveDirectoryContextFactory adFactory)
        {
            //User = httpContextAccessor.HttpContext.User;
            Directory = adFactory.CreateActiveDirectoryContext();
            RequestId = Guid.NewGuid();
            ResponseData.Add("Request Id", RequestId);
            ResponseData.Add("Version", "1.0");
            ResponseData.Add("Received Time", _startTime);
            ResponseData.Add("User", httpContextAccessor?.HttpContext?.User?.Identity?.Name);
            ResponseData.Add("User Id", httpContextAccessor?.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value);
            ResponseData.Add("IP Address", httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString());

        }
        //[HttpGet("badrequest")] // Add a route attribute
        //public IActionResult BadRequest()
        //{
        //    return new BadRequestResult();
        //}
        protected IActiveDirectoryContext Directory { get; }
        protected Guid RequestId { get; }

        protected IActionResult FormatData(dynamic data)
        {
            ResponseData.Add("Data", data);
            ResponseData.Add("Finish Time", DateTime.Now.ToString());
            ResponseData.Add("Runtime", (DateTime.Now - _startTime).TotalMilliseconds + "ms");

            return new JsonResult(ResponseData);
        }
    }
}
