using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using BLAZAM.Services.Audit;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Pages.API.v1
{
    /// <summary>
    /// Base class for all API controllers that contains common
    /// shared elements that make the API work
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Login)]
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    public class ApiController : Controller
    {
        private DateTime _startTime = DateTime.Now;

        /// <summary>
        /// A string dictionary that contains the base of the response.
        /// </summary>
        protected Dictionary<string, object?> ResponseData = new();
        /// <summary>
        /// A factory for <see cref="IDatabaseContext"/> connections
        /// </summary>
        protected readonly IAppDatabaseFactory DbFactory;
        /// <summary>
        /// The API audit logger
        /// </summary>
        protected readonly AuditLogger AuditLogger;
        /// <summary>
        /// 
        /// </summary>
        protected readonly IApplicationUserStateService UserStateService;

        /// <summary>
        /// The current API user state
        /// </summary>
        protected IApplicationUserState? CurrentUserState { get; }

        public ApiController(IApplicationUserStateService applicationUserStateService, AuditLogger audit, IAppDatabaseFactory appDatabaseFactory, IHttpContextAccessor httpContextAccessor, IActiveDirectoryContextFactory adFactory)
        {
            //User = httpContextAccessor.HttpContext.User;
            AuditLogger = audit;
            UserStateService = applicationUserStateService;
            CurrentUserState = UserStateService.CurrentUserState;

            Directory = adFactory.CreateActiveDirectoryContext();
            DbFactory = appDatabaseFactory;
            RequestId = Guid.NewGuid();
            ResponseData.Add("Request Id", RequestId);
            ResponseData.Add("Version", "1.0");
            ResponseData.Add("Received Time", _startTime);
            ResponseData.Add("User", httpContextAccessor?.HttpContext?.User?.Identity?.Name);
            ResponseData.Add("User Id", httpContextAccessor?.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value);
            ResponseData.Add("IP Address", httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString());

        }
        /// <summary>
        /// The current API users Active Directory connection
        /// </summary>
        protected IActiveDirectoryContext Directory { get; }
        /// <summary>
        /// A unique ID for the execution of this controller
        /// </summary>
        protected Guid RequestId { get; }


        /// <summary>
        /// Returns a JSON response with the data and footer
        /// fields appended
        /// </summary>
        /// <param name="data">A JSON serializable object</param>
        /// <returns>A new <see cref="JsonResult"/> containing the <see cref="ResponseData"/></returns>
        protected IActionResult FormatData(dynamic data)
        {
            ResponseData.Add("Data", data);
            ResponseData.Add("Finish Time", DateTime.Now.ToString());
            ResponseData.Add("Runtime", (DateTime.Now - _startTime).TotalMilliseconds + "ms");

            return new JsonResult(ResponseData);
        }
    }
}
