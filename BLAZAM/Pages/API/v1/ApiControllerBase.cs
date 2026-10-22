using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Services.Audit;

using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    public class ApiControllerBase : ControllerBase
    {
        private readonly Stopwatch _stopwatch = new();

        /// <summary>
        /// A string dictionary that contains the base of the response.
        /// </summary>
        protected Dictionary<string, object?> ResponseData = [];
        /// <summary>
        /// A factory for <see cref="IDatabaseContext"/> connections
        /// </summary>
        protected readonly IUserDatabaseFactory DbFactory;
        /// <summary>
        /// The API audit logger
        /// </summary>
        protected readonly WebUserAuditLogger AuditLogger;
        /// <summary>
        /// 
        /// </summary>
        protected readonly IApplicationUserStateService UserStateService;

        /// <summary>
        /// The current API user state
        /// </summary>
        protected IApplicationUserState? CurrentUserState { get; }

        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Creates a new API controller with the required services
        /// </summary>
        /// <param name="applicationUserStateService"></param>
        /// <param name="audit"></param>
        /// <param name="appDatabaseFactory"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="adFactory"></param>
        public ApiControllerBase(IApplicationUserStateService applicationUserStateService, WebUserAuditLogger audit, IUserDatabaseFactory appDatabaseFactory, IHttpContextAccessor httpContextAccessor, IActiveDirectoryContextFactory adFactory)
        {
            _jsonOptions = new JsonSerializerOptions();
            _jsonOptions.Converters.Add(new JsonStringEnumConverter());

            _stopwatch.Start();

            RequestId = Guid.NewGuid();
            ResponseData.Add("Request Id", RequestId);
            ResponseData.Add("Version", "1.0");
            ResponseData.Add("Received Time", DateTime.UtcNow);
            ResponseData.Add("User", httpContextAccessor?.HttpContext?.User?.Identity?.Name);
            ResponseData.Add("User Id", httpContextAccessor?.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value);
            ResponseData.Add("IP Address", httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString());


            AuditLogger = audit;
            UserStateService = applicationUserStateService;
            CurrentUserState = UserStateService.CurrentUserState;

            Directory = adFactory.CreateActiveDirectoryContext(CurrentUserState.ToActiveDirectoryUserState());
            DbFactory = appDatabaseFactory;


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
        protected IActionResult FormatData(object data)
        {
            _stopwatch.Stop();


            if (data is IEnumerable<object> dataEnumerable)
            {


                // Serialize each item using its runtime type
                var serializedItems = dataEnumerable
                    .Select(item => JsonSerializer.SerializeToElement(item, item.GetType(), _jsonOptions))
                    .ToList();


                ResponseData["Data"] = serializedItems;
            }
            else
            {
                ResponseData["Data"] = data;
            }

            ResponseData.Add("Finish Time", DateTime.Now.ToString());
            ResponseData.Add("Runtime", _stopwatch.ElapsedMilliseconds + "ms");

            return new JsonResult(ResponseData);
        }
        /// <summary>
        /// Finds a group by its SID, DN, or name. If using name, the name must be unique
        /// </summary>
        /// <param name="groupIdentifier"></param>
        /// <returns></returns>
        /// <exception cref="DirectorySearchUniquenessException"></exception>
        protected IADGroup? FindGroupByIdentifier(string groupIdentifier)
        {
            var group = (IADGroup?)Directory.FindGlobalEntryBySid(groupIdentifier);
            group ??= (IADGroup?)Directory.GetDirectoryEntryByDN(groupIdentifier);
            if (group == null)
            {
                var matches = Directory.Groups.FindGroupByString(groupIdentifier, true);
                if (matches != null && matches.Count > 0)
                {
                    if (matches.Count > 1)
                    {
                        throw new DirectorySearchUniquenessException(groupIdentifier);
                    }

                    group = matches[0];
                }
            }


            return group;
        }

        /// <summary>
        /// Finds a user by SID, DN, username, or string search.
        /// </summary>
        /// <param name="userIdentifier">The SID, DN, username, or string identifier</param>
        /// <returns>The matching user or null</returns>
        /// <exception cref="DirectorySearchUniquenessException"></exception>
        protected IADUser? FindUserByIdentifier(string userIdentifier)
        {
            var user = (IADUser?)Directory.FindGlobalEntryBySid(userIdentifier);
            user ??= (IADUser?)Directory.GetDirectoryEntryByDN(userIdentifier);
            user ??= Directory.Users.FindUserByUsername(userIdentifier, ignoreDisabledUsers: false, exactMatch: true);
            if (user == null)
            {
                var matches = Directory.Users.FindUsersByString(userIdentifier, ignoreDisabledUsers: false, exactMatch: true);
                if (matches != null && matches.Count > 0)
                {
                    if (matches.Count > 1)
                    {
                        throw new DirectorySearchUniquenessException(userIdentifier);
                    }

                    user = matches[0];
                }
            }

            return user;
        }

        /// <summary>
        /// Finds a computer by SID, DN, or computer name.
        /// </summary>
        /// <param name="computerIdentifier">The SID, DN, or computer name</param>
        /// <returns>The matching computer or null</returns>
        /// <exception cref="DirectorySearchUniquenessException"></exception>
        protected IADComputer? FindComputerByIdentifier(string computerIdentifier)
        {
            var computer = (IADComputer?)Directory.FindGlobalEntryBySid(computerIdentifier);
            computer ??= (IADComputer?)Directory.GetDirectoryEntryByDN(computerIdentifier);
            if (computer == null)
            {
                var matches = Directory.Computers.FindByString(computerIdentifier, ignoreDisabled: false);
                if (matches != null && matches.Count > 0)
                {
                    var exact = matches.Where(c => string.Equals(c.CanonicalName, computerIdentifier, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(c.SAMAccountName, computerIdentifier, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(c.SAMAccountName, computerIdentifier + "$", StringComparison.OrdinalIgnoreCase)).ToList();

                    if (exact.Count == 1)
                    {
                        computer = exact[0];
                    }
                    else if (exact.Count > 1 || matches.Count > 1)
                    {
                        throw new DirectorySearchUniquenessException(computerIdentifier);
                    }
                    else
                    {
                        computer = matches[0];
                    }
                }
            }

            return computer;
        }

        /// <summary>
        /// Finds an Organizational Unit by DN or name.
        /// </summary>
        /// <param name="ouIdentifier">The DN or OU name</param>
        /// <returns>The matching Organizational Unit or null</returns>
        /// <exception cref="DirectorySearchUniquenessException"></exception>
        protected IADOrganizationalUnit? FindOuByIdentifier(string ouIdentifier)
        {
            var ou = Directory.OUs.FindOuByDN(ouIdentifier);
            if (ou == null)
            {
                var matches = Directory.OUs.FindOuByString(ouIdentifier);
                if (matches != null && matches.Count > 0)
                {
                    if (matches.Count > 1)
                    {
                        throw new DirectorySearchUniquenessException(ouIdentifier);
                    }

                    ou = matches[0];
                }
            }

            return ou;
        }
    }
}
