using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Services.Audit;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BLAZAM.Pages.API.v1
{
    /// <summary>
    /// API controller for managing Active Directory computers.
    /// </summary>
    public class Computer : ApiControllerBase
    {
        /// <summary>
        /// Constructor for the Computer API controller.
        /// </summary>
        public Computer(IApplicationUserStateService applicationUserStateService,
            WebUserAuditLogger audit,
            IUserDatabaseFactory appDatabaseFactory,
            IHttpContextAccessor httpContextAccessor,
            IActiveDirectoryContextFactory adFactory)
            : base(applicationUserStateService, audit, appDatabaseFactory, httpContextAccessor, adFactory)
        {
        }

        /// <summary>
        /// Gets computer details by SID, DN, or computer name.
        /// </summary>
        /// <param name="identifier">The computer identifier (SID, DN, or computer name)</param>
        /// <response code="200">Returns the computer object.</response>
        /// <response code="403">Forbidden - The user does not have permission to view this computer.</response>
        /// <response code="404">Not Found - The specified computer was not found.</response>
        [HttpGet("{identifier}")]
        public IActionResult Get(string identifier)
        {
            try
            {
                var computer = FindComputerByIdentifier(identifier);
                if (computer == null)
                {
                    return NotFound("Computer not found");
                }
                if (!computer.CanRead)
                {
                    return Forbid("You do not have permission to view this computer");
                }
                return FormatData(computer);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Disables a computer account.
        /// </summary>
        /// <param name="identifier">The computer identifier</param>
        /// <response code="200">Returns the disabled computer object.</response>
        /// <response code="403">Forbidden - The user does not have permission to disable this computer.</response>
        /// <response code="404">Not Found - The computer was not found.</response>
        [HttpPost("{identifier}/disable")]
        public async Task<IActionResult> Disable(string identifier)
        {
            try
            {
                var computer = FindComputerByIdentifier(identifier);
                if (computer == null)
                {
                    return NotFound("Computer not found");
                }
                if (!computer.CanDisable)
                {
                    return Forbid("You do not have permission to disable this computer");
                }

                computer.Disabled = true;
                var job = await computer.CommitChangesAsync();
                if (job.FailedSteps.Count > 0)
                {
                    return UnprocessableEntity(job.FailedSteps.Select(s => s.Exception?.InnerException?.Message ?? s.Exception?.Message));
                }

                return FormatData(computer);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Enables a computer account.
        /// </summary>
        /// <param name="identifier">The computer identifier</param>
        /// <response code="200">Returns the enabled computer object.</response>
        /// <response code="403">Forbidden - The user does not have permission to enable this computer.</response>
        /// <response code="404">Not Found - The computer was not found.</response>
        [HttpPost("{identifier}/enable")]
        public async Task<IActionResult> Enable(string identifier)
        {
            try
            {
                var computer = FindComputerByIdentifier(identifier);
                if (computer == null)
                {
                    return NotFound("Computer not found");
                }
                if (!computer.CanEnable)
                {
                    return Forbid("You do not have permission to enable this computer");
                }

                computer.StageEnable();
                var job = await computer.CommitChangesAsync();
                if (job.FailedSteps.Count > 0)
                {
                    return UnprocessableEntity(job.FailedSteps.Select(s => s.Exception?.InnerException?.Message ?? s.Exception?.Message));
                }

                return FormatData(computer);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a computer account.
        /// </summary>
        /// <param name="identifier">The computer identifier</param>
        /// <response code="200">Computer account successfully deleted.</response>
        /// <response code="403">Forbidden - The user does not have permission to delete this computer.</response>
        /// <response code="404">Not Found - The computer was not found.</response>
        [HttpDelete("{identifier}")]
        public IActionResult DeleteComputer(string identifier)
        {
            try
            {
                var computer = FindComputerByIdentifier(identifier);
                if (computer == null)
                {
                    return NotFound("Computer not found");
                }
                if (!computer.CanDelete)
                {
                    return Forbid("You do not have permission to delete this computer");
                }

                computer.Delete();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
