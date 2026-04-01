using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Models.Templates;
using BLAZAM.EmailMessage.Email.Messages;
using BLAZAM.Gui.Helpers;
using BLAZAM.Jobs;
using BLAZAM.Localization;
using BLAZAM.Pages.API.Data;
using BLAZAM.Services.Audit;
using BLAZAM.Services.Events;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using MudBlazor;
using System.Security;
using System.Text.Json;

namespace BLAZAM.Pages.API.v1
{
    /// <summary>
    /// Template API endpoints provide listing of templates
    /// and execution to create users.
    /// </summary>
    /// <remarks>
    /// Constructs a new instance of the Templates API controller.
    /// </remarks>
    /// <param name="email"></param>
    /// <param name="applicationUserStateService"></param>
    /// <param name="localizer"></param>
    /// <param name="audit"></param>
    /// <param name="appDatabaseFactory"></param>
    /// <param name="httpContextAccessor"></param>
    /// <param name="adFactory"></param>
    [Authorize(Roles = UserRoles.SuperAdmin)]
    public class Templates(EmailService email,
        IApplicationUserStateService applicationUserStateService,
        IStringLocalizer<AppLocalization> localizer,
        WebUserAuditLogger audit,
        IUserDatabaseFactory appDatabaseFactory,
        IHttpContextAccessor httpContextAccessor,
        IActiveDirectoryContextFactory adFactory) : ApiControllerBase(applicationUserStateService, audit, appDatabaseFactory, httpContextAccessor, adFactory)
    {
        private readonly IStringLocalizer<AppLocalization> _appLocalization = localizer;
        private readonly EmailService _emailService = email;



        /// <summary>
        /// Executes a user creation template. Any required fields will need to be
        /// provided in post body data.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/v1/templates/execute/2
        ///     {
        ///       "firstName": "Test",
        ///       "lastName": "User",
        ///       "fields": [
        ///          {
        ///            "FieldName": "l",
        ///            "FieldValue": "Boston"
        ///         }
        ///       ]
        ///       "groups": [
        ///          {
        ///            "S-1-5-21-1004336348-1177238915-682003330-512",
        ///            "CN=Group Name,OU=Somewhere,DC=example,DC=org",
        ///            "Another Group Name"
        ///         }
        ///       ]
        ///     }
        ///
        /// </remarks>
        /// <param name="templateId">The ID of the template to execute.</param>
        /// <param name="newUserDetails">A complete NewUserDetails request schema</param>
        /// <response code="200">Returns the DN of the created user.</response>
        /// <response code="401">Unauthorized - The user is not authenticated.</response>
        /// <response code="403">Forbidden - The user does not have the required role.</response>
        /// <response code="422">Unprocessable - The creation request cannot be processed due to an internal error.</response>
        [HttpPost]
        [Route("execute/{templateId}")]

        public async Task<IActionResult> Execute(int templateId, [FromBody] NewUserPayload newUserDetails)
        {

            var context = await DbFactory.CreateDbContextAsync();
            var template = await context.DirectoryTemplates.Include(t => t.ParentTemplate).FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null)
            {
                return new NotFoundObjectResult(templateId);
            }

            try
            {
                ValidateInput(newUserDetails, template);

            }
            catch (BadHttpRequestException ex)
            {
                return new BadRequestObjectResult(ex.Message);

            }


            //Prepare new user name
            var newUserName = new NewUserName()
            {
                GivenName = newUserDetails.FirstName,
                MiddleName = newUserDetails.MiddleName,
                Surname = newUserDetails.LastName
            };
            try
            {
                var customOU = Directory.OUs.FindOuByDN(newUserDetails.OU);
                if (customOU == null && template.EffectiveParentOU == null)
                {
                    return new BadRequestObjectResult("There was no OU provided by API call or template!");
                }
                //Generate IADUser
                var newUser = await template.GenerateTemplateUserAsync(newUserName, Directory, customOU);
                if (newUser == null)
                {
                    return new UnprocessableEntityObjectResult("Could not generate user from template");
                }
                //Override username if provided
                if (!newUserDetails.Username.IsNullOrEmpty())
                {
                    newUser.SAMAccountName = newUserDetails.Username;
                }

                //Store password in memory for later
                var password = newUser.NewPassword.ToPlainText().ToSecureString();

                //Set each field in template
                template.PopulateFields(newUser, newUserName);

                //Set API provided fields
                SetFields(newUserDetails, newUser);

                //Set API provided groups
                AssignGroups(newUserDetails, newUser);

                //Prepare commit job
                Job createUserJob = new(_appLocalization[Lang.Create_User])
                {
                    StopOnFailedStep = true
                };

                //Commmit
                var result = await newUser.CommitChangesAsync(createUserJob);

                if (result.FailedSteps.Count > 0)
                {
                    return new UnprocessableEntityObjectResult(result.FailedSteps.Select(s => s.Exception?.InnerException != null ? s.Exception.InnerException.Message : s.Exception?.Message));
                }


                newUser = (IADUser)Directory.GetDirectoryEntryByDN(newUser.DN);

                await AuditAndNotify(newUserDetails, template, newUser, password);

                return new CreatedResult(newUser.OU, newUser.DN);
            }
            catch (DirectorySearchUniquenessException ex)
            {
                return new UnprocessableEntityObjectResult("Multiple groups match the provided search term: " + ex.SearchTerm);
            }
            catch (Exception ex)
            {
                return new UnprocessableEntityObjectResult(ex.Message);
            }

        }

        private async Task AuditAndNotify(NewUserPayload newUserDetails, DirectoryTemplate? template, IADUser entry, SecureString password)
        {
            ApplicationEvents.DirectoryEntryEvent.Invoke(new()
            {
                EventType = ApplicationEventType.Create,
                Entry = entry,
                Actor = CurrentUserState

            });



            if (template?.EffectiveSendWelcomeEmail == true)
            {
                if (template.EffectiveAskForAlternateEmail == true || entry.Email.IsNullOrEmpty())
                {

                    await SendWelcomeEmail(entry, newUserDetails.SendWelcomeEmailTo!, password);

                }
                else
                {
                    await SendWelcomeEmail(entry, entry.Email!, password);
                }
            }

        }

        private static void SetFields(NewUserPayload newUserDetails, IADUser? newUser)
        {
            if (newUserDetails.Fields != null)
            {
                foreach (var field in newUserDetails.Fields)
                {
                    var json = field.FieldValue as JsonElement?;
                    if (json == null)
                    {
                        continue;
                    }
                    var kind = json.Value.ValueKind;
                    object? value = null;
                    switch (kind)
                    {
                        case JsonValueKind.String:
                            value = json.Value.GetString(); break;
                        case JsonValueKind.Number:
                            value = json.Value.GetDouble(); break;
                        case JsonValueKind.False:
                        case JsonValueKind.True:
                            value = json.Value.GetBoolean(); break;

                    }

                    newUser?.SetCustomProperty(field.FieldName, value);
                }
            }
        }

        private void AssignGroups(NewUserPayload newUserDetails, IADUser? newUser)
        {
            if (newUserDetails.Groups != null)
            {
                foreach (var groupIdentifier in newUserDetails.Groups)
                {
                    var group = FindGroupByIdentifier(groupIdentifier);
                    if (group != null)
                    {
                        newUser?.AssignTo(group);
                    }
                }
            }
        }

        private static void ValidateInput(NewUserPayload newUserDetails, DirectoryTemplate? template)
        {
            //Check if the request has the required fields for this template
            if (newUserDetails.FirstName.AppTrim().IsNullOrEmpty())
            {
                throw new BadHttpRequestException("FirstName is required");
            }
            if (newUserDetails.LastName.AppTrim().IsNullOrEmpty())
            {
                throw new BadHttpRequestException("LastName is required");
            }
            if (template?.HasRequiredFields() == true)
            {
                var requiredFields = template.EffectiveFieldValues.Where(fv => fv.Required).ToList();
                var providedFieldNames = newUserDetails.Fields?.Select(field => field.FieldName).ToList();

                foreach (var field in requiredFields)
                {
                    //If any are missing return an error with explanation
                    if (!providedFieldNames?.Any(f => f.Equals(field.FieldName, StringComparison.InvariantCultureIgnoreCase)) == true)
                    {
                        throw new BadHttpRequestException(field.FieldName + " is a required field");
                    }
                }
            }

        }


        /// <summary>
        /// Returns all user creation templates the user has access to.
        /// </summary>
        /// <response code="200">Returns a list of user creation templates.</response>
        /// <response code="401">Unauthorized - The user is not authenticated.</response>
        /// <response code="403">Forbidden - The user does not have the required role.</response>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using var context = await DbFactory.CreateDbContextAsync();
            var list = await context.DirectoryTemplates.Where(t => t.DeletedAt == null && t.Visible).ToListAsync();
            return FormatData(list);

        }

        private async Task SendWelcomeEmail(IADUser user, string to, SecureString password)
        {
            try
            {
                NewUserWelcomeEmailMessage message = new()
                {
                    Domain = user.Directory.ConnectionSettings?.FQDN,
                    Username = user.SAMAccountName,
                    Password = password
                };
                await _emailService.SendMessage(_appLocalization["New Account Details"], message, to);

            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Error sending welcome email");
            }
        }
    }
}
