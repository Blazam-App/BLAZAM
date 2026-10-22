using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Pages.API.Data;
using BLAZAM.Services.Audit;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BLAZAM.Pages.API.v1
{
    /// <summary>
    /// API controller for managing Active Directory users.
    /// </summary>
    public class User : ApiControllerBase
    {
        /// <summary>
        /// Constructor for the User API controller.
        /// </summary>
        public User(IApplicationUserStateService applicationUserStateService,
            WebUserAuditLogger audit,
            IUserDatabaseFactory appDatabaseFactory,
            IHttpContextAccessor httpContextAccessor,
            IActiveDirectoryContextFactory adFactory)
            : base(applicationUserStateService, audit, appDatabaseFactory, httpContextAccessor, adFactory)
        {
        }

        /// <summary>
        /// Gets a user by SID, DN, username, or name.
        /// </summary>
        /// <param name="identifier">The user identifier (SID, DN, or SAMAccountName)</param>
        /// <response code="200">Returns the user object.</response>
        /// <response code="401">Unauthorized - The user is not authenticated.</response>
        /// <response code="403">Forbidden - The user does not have read access to this entry.</response>
        /// <response code="404">Not Found - The specified user was not found.</response>
        [HttpGet("{identifier}")]
        public IActionResult Get(string identifier)
        {
            try
            {
                var user = FindUserByIdentifier(identifier);
                if (user == null)
                {
                    return NotFound("User not found");
                }
                if (!user.CanRead)
                {
                    return Forbid("You do not have permission to view this user");
                }
                return FormatData(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates a user's details and custom fields.
        /// </summary>
        /// <param name="identifier">The user identifier</param>
        /// <param name="payload">The updated user properties</param>
        /// <response code="200">Returns the updated user object.</response>
        /// <response code="400">Bad Request - Malformed request or execution error.</response>
        /// <response code="403">Forbidden - The user does not have permission to edit this user.</response>
        /// <response code="404">Not Found - The user was not found.</response>
        [HttpPut("{identifier}")]
        public async Task<IActionResult> Update(string identifier, [FromBody] UpdateUserPayload payload)
        {
            try
            {
                var user = FindUserByIdentifier(identifier);
                if (user == null)
                {
                    return NotFound("User not found");
                }
                if (!user.CanEdit)
                {
                    return Forbid("You do not have permission to edit this user");
                }

                if (payload.GivenName != null) user.GivenName = payload.GivenName;
                if (payload.Surname != null) user.Sn = payload.Surname;
                if (payload.MiddleName != null) user.MiddleName = payload.MiddleName;
                if (payload.Email != null) user.Email = payload.Email;
                if (payload.Description != null) user.Description = payload.Description;

                if (payload.Fields != null)
                {
                    foreach (var field in payload.Fields)
                    {
                        var json = field.FieldValue as JsonElement?;
                        object? value = json.HasValue ? GetJsonValue(json.Value) : field.FieldValue;
                        user.SetCustomProperty(field.FieldName, value);
                    }
                }

                var job = await user.CommitChangesAsync();
                if (job.FailedSteps.Count > 0)
                {
                    return UnprocessableEntity(job.FailedSteps.Select(s => s.Exception?.InnerException?.Message ?? s.Exception?.Message));
                }

                return FormatData(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Disables a user account.
        /// </summary>
        /// <param name="identifier">The user identifier</param>
        /// <response code="200">Returns the disabled user object.</response>
        /// <response code="403">Forbidden - The user does not have permission to disable this account.</response>
        /// <response code="404">Not Found - The user was not found.</response>
        [HttpPost("{identifier}/disable")]
        public async Task<IActionResult> Disable(string identifier)
        {
            try
            {
                var user = FindUserByIdentifier(identifier);
                if (user == null)
                {
                    return NotFound("User not found");
                }
                if (!user.CanDisable)
                {
                    return Forbid("You do not have permission to disable this user");
                }

                user.Disabled = true;
                var job = await user.CommitChangesAsync();
                if (job.FailedSteps.Count > 0)
                {
                    return UnprocessableEntity(job.FailedSteps.Select(s => s.Exception?.InnerException?.Message ?? s.Exception?.Message));
                }

                return FormatData(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Enables a user account.
        /// </summary>
        /// <param name="identifier">The user identifier</param>
        /// <response code="200">Returns the enabled user object.</response>
        /// <response code="403">Forbidden - The user does not have permission to enable this account.</response>
        /// <response code="404">Not Found - The user was not found.</response>
        [HttpPost("{identifier}/enable")]
        public async Task<IActionResult> Enable(string identifier)
        {
            try
            {
                var user = FindUserByIdentifier(identifier);
                if (user == null)
                {
                    return NotFound("User not found");
                }
                if (!user.CanEnable)
                {
                    return Forbid("You do not have permission to enable this user");
                }

                user.StageEnable();
                var job = await user.CommitChangesAsync();
                if (job.FailedSteps.Count > 0)
                {
                    return UnprocessableEntity(job.FailedSteps.Select(s => s.Exception?.InnerException?.Message ?? s.Exception?.Message));
                }

                return FormatData(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Unlocks a locked user account.
        /// </summary>
        /// <param name="identifier">The user identifier</param>
        /// <response code="200">Returns the unlocked user object.</response>
        /// <response code="403">Forbidden - The user does not have permission to unlock this account.</response>
        /// <response code="404">Not Found - The user was not found.</response>
        [HttpPost("{identifier}/unlock")]
        public async Task<IActionResult> Unlock(string identifier)
        {
            try
            {
                var user = FindUserByIdentifier(identifier);
                if (user == null)
                {
                    return NotFound("User not found");
                }
                if (!user.CanUnlock)
                {
                    return Forbid("You do not have permission to unlock this user");
                }

                user.LockedOut = false;
                var job = await user.CommitChangesAsync();
                if (job.FailedSteps.Count > 0)
                {
                    return UnprocessableEntity(job.FailedSteps.Select(s => s.Exception?.InnerException?.Message ?? s.Exception?.Message));
                }

                return FormatData(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Resets a user's password.
        /// </summary>
        /// <param name="identifier">The user identifier</param>
        /// <param name="payload">The password payload</param>
        /// <response code="200">Returns the user object after password reset.</response>
        /// <response code="403">Forbidden - The user does not have permission to reset passwords for this user.</response>
        /// <response code="404">Not Found - The user was not found.</response>
        [HttpPost("{identifier}/reset-password")]
        public async Task<IActionResult> ResetPassword(string identifier, [FromBody] ResetPasswordPayload payload)
        {
            try
            {
                var user = FindUserByIdentifier(identifier);
                if (user == null)
                {
                    return NotFound("User not found");
                }
                if (!user.CanSetPassword)
                {
                    return Forbid("You do not have permission to reset the password for this user");
                }

                if (string.IsNullOrEmpty(payload?.NewPassword))
                {
                    return BadRequest("NewPassword is required.");
                }

                user.StagePasswordChange(payload.NewPassword.ToSecureString(), payload.RequireChangeAtNextLogon);
                var job = await user.CommitChangesAsync();
                if (job.FailedSteps.Count > 0)
                {
                    return UnprocessableEntity(job.FailedSteps.Select(s => s.Exception?.InnerException?.Message ?? s.Exception?.Message));
                }

                return FormatData(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a user account.
        /// </summary>
        /// <param name="identifier">The user identifier</param>
        /// <response code="200">User successfully deleted.</response>
        /// <response code="403">Forbidden - The user does not have permission to delete this user.</response>
        /// <response code="404">Not Found - The user was not found.</response>
        [HttpDelete("{identifier}")]
        public IActionResult DeleteUser(string identifier)
        {
            try
            {
                var user = FindUserByIdentifier(identifier);
                if (user == null)
                {
                    return NotFound("User not found");
                }
                if (!user.CanDelete)
                {
                    return Forbid("You do not have permission to delete this user");
                }

                user.Delete();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private static object? GetJsonValue(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => element.GetRawText()
            };
        }
    }
}
