using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Pages.API.Data;
using BLAZAM.Services.Audit;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BLAZAM.Pages.API.v1
{
    /// <summary>
    /// Manages Organizational Units in Active Directory.
    /// </summary>
    public class OU : ApiControllerBase
    {
        /// <summary>
        /// Constructor for the OU API controller.
        /// </summary>
        public OU(IApplicationUserStateService applicationUserStateService,
            WebUserAuditLogger audit,
            IUserDatabaseFactory appDatabaseFactory,
            IHttpContextAccessor httpContextAccessor,
            IActiveDirectoryContextFactory adFactory)
            : base(applicationUserStateService, audit, appDatabaseFactory, httpContextAccessor, adFactory)
        {
        }

        /// <summary>
        /// Gets the application root Organizational Unit.
        /// </summary>
        /// <response code="200">Returns the root OU object.</response>
        /// <response code="403">Forbidden - The user does not have permission to view the root OU.</response>
        [HttpGet]
        public IActionResult GetRoot()
        {
            try
            {
                var rootOu = Directory.OUs.GetApplicationRootOU();
                if (rootOu == null)
                {
                    return NotFound("Root OU not found");
                }
                if (!rootOu.CanRead)
                {
                    return Forbid("You do not have permission to view the root OU");
                }
                return FormatData(rootOu);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets an Organizational Unit by DN or name.
        /// </summary>
        /// <param name="identifier">The OU identifier (DN or name)</param>
        /// <response code="200">Returns the OU details.</response>
        /// <response code="403">Forbidden - The user does not have permission to view this OU.</response>
        /// <response code="404">Not Found - The specified OU was not found.</response>
        [HttpGet("{identifier}")]
        public IActionResult Get(string identifier)
        {
            try
            {
                var ou = FindOuByIdentifier(identifier);
                if (ou == null)
                {
                    return NotFound("OU not found");
                }
                if (!ou.CanRead)
                {
                    return Forbid("You do not have permission to view this OU");
                }
                return FormatData(ou);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets all child Organizational Units under the specified OU.
        /// </summary>
        /// <param name="identifier">The OU identifier</param>
        /// <response code="200">Returns a list of child OUs.</response>
        /// <response code="403">Forbidden - The user does not have permission to view this OU.</response>
        /// <response code="404">Not Found - The specified OU was not found.</response>
        [HttpGet("{identifier}/subous")]
        public IActionResult GetSubOus(string identifier)
        {
            try
            {
                var ou = FindOuByIdentifier(identifier);
                if (ou == null)
                {
                    return NotFound("OU not found");
                }
                if (!ou.CanRead)
                {
                    return Forbid("You do not have permission to view this OU");
                }

                var subOus = Directory.OUs.FindSubOusByDN(ou.DN).Where(s => s.CanRead).ToList();
                return FormatData(subOus);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Creates a new Organizational Unit.
        /// </summary>
        /// <param name="payload">The new OU details</param>
        /// <response code="200">Returns the newly created OU object.</response>
        /// <response code="400">Bad Request - Malformed request or execution error.</response>
        /// <response code="403">Forbidden - The user does not have permission to create OUs in the parent OU.</response>
        /// <response code="404">Not Found - The parent OU was not found.</response>
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] NewOuPayload payload)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(payload?.Name) || string.IsNullOrWhiteSpace(payload?.ParentOU))
                {
                    return BadRequest("Name and ParentOU are required.");
                }

                var parentOu = FindOuByIdentifier(payload.ParentOU);
                if (parentOu == null)
                {
                    return NotFound("Parent OU not found");
                }
                if (!parentOu.CanCreateType(ActiveDirectoryObjectType.OU) && !parentOu.CanCreate)
                {
                    return Forbid("You do not have permission to create OUs in this parent OU");
                }

                var newOu = parentOu.CreateOU(payload.Name);
                var job = await newOu.CommitChangesAsync();
                if (job.FailedSteps.Count > 0)
                {
                    return UnprocessableEntity(job.FailedSteps.Select(s => s.Exception?.InnerException?.Message ?? s.Exception?.Message));
                }

                return FormatData(newOu);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Deletes an Organizational Unit.
        /// </summary>
        /// <param name="identifier">The OU identifier</param>
        /// <response code="200">OU successfully deleted.</response>
        /// <response code="403">Forbidden - The user does not have permission to delete this OU.</response>
        /// <response code="404">Not Found - The OU was not found.</response>
        [HttpDelete("{identifier}")]
        public IActionResult DeleteOu(string identifier)
        {
            try
            {
                var ou = FindOuByIdentifier(identifier);
                if (ou == null)
                {
                    return NotFound("OU not found");
                }
                if (!ou.CanDelete)
                {
                    return Forbid("You do not have permission to delete this OU");
                }

                ou.Delete();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
