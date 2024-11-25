using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Pages.API.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using AngleSharp.Html.Construction;
using BLAZAM.Common.Data;
using Octokit;
using BLAZAM.EmailMessage.Email.Notifications;
using MudBlazor;
using System.Security;
using Microsoft.Extensions.Localization;
using BLAZAM.Localization;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Database.Models.Templates;
using BLAZAM.Jobs;
using BLAZAM.Services.Audit;
using BLAZAM.Session.Interfaces;
using System.Text.Json;

namespace BLAZAM.Pages.API.v1
{
    public class Templates : ApiController
    {
        private IAppDatabaseFactory _appDatabaseFactory;
        private IStringLocalizer<AppLocalization> AppLocalization;
        private EmailService EmailService;
        private NotificationGenerationService OUNotificationService;

        public Templates(NotificationGenerationService ouNotificationService, EmailService email, IApplicationUserStateService applicationUserStateService, IStringLocalizer<AppLocalization> localizer, AuditLogger audit, IAppDatabaseFactory appDatabaseFactory, IHttpContextAccessor httpContextAccessor, IActiveDirectoryContextFactory adFactory) : base(applicationUserStateService, audit, appDatabaseFactory, httpContextAccessor, adFactory)
        {
            AppLocalization = localizer;
            EmailService = email;
            OUNotificationService = ouNotificationService;
        }



        /// <summary>
        /// Executes a user creation template. Any required fields will need to be provided in form data.
        /// </summary>
        ///  /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/v1/templates/execute/2
        ///     {
        ///       "firstName": "Test",
        ///       "lastName": "User",
        ///       "fields": [
        ///          {
        ///            "FieldName": "l",
        ///           "FieldValue": "Boston"
        ///         }
        ///       ]
        ///       "groups": [
        ///          {
        ///            "S-1-5-21-1004336348-1177238915-682003330-512"
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
        [Route("/api/v1/templates/execute/{templateId}")]

        public async Task<IActionResult> Execute(int templateId, [FromBody] NewUserDetails newUserDetails)
        {
            //newUserDetails.Fields.Add(new() { FieldName = "test", FieldValue = "val" });
            //var test = JsonConvert.SerializeObject(newUserDetails);
            
            var context = await DbFactory.CreateDbContextAsync();
            var template = await context.DirectoryTemplates.Include(t => t.ParentTemplate).FirstOrDefaultAsync(t => t.Id == templateId);
            
            if (template != null)
            {
                if (template.HasRequiredFields())
                {
                    var requiredFields = template.EffectiveFieldValues.Where(fv => fv.Required).ToList();
                    foreach (var field in requiredFields)
                    {
                        if (!newUserDetails.Fields.Any(f => f.FieldName.Equals(field.FieldName, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            return new BadRequestObjectResult(field.FieldName + " is a required field");
                        }

                    }
                }

                var newUserName = new NewUserName()
                {
                    GivenName = newUserDetails.FirstName,
                    MiddleName = newUserDetails.MiddleName,
                    Surname = newUserDetails.LastName
                };
             
                var newUser = template.GenerateTemplateUser(newUserName, Directory);
                if (!newUserDetails.Username.IsNullOrEmpty())
                {
                    newUser.SamAccountName = newUserDetails.Username;
                }
                var password = newUser.NewPassword.ToPlainText().ToSecureString();
                foreach (var fieldValue in template.EffectiveFieldValues)
                {
                    try
                    {
                        if (fieldValue.Field != null && fieldValue.Value != null)
                            if (fieldValue.Field.FieldName.ToLower() == "homedirectory")
                                newUser.HomeDirectory = template.ReplaceVariables(fieldValue.Value, newUserName, newUser.SamAccountName);
                            else
                                newUser.NewEntryProperties[fieldValue.Field.FieldName] = template.ReplaceVariables(fieldValue.Value, newUserName, newUser.SamAccountName);
                        else if (fieldValue.CustomField != null && fieldValue.Value != null)
                            newUser.NewEntryProperties[fieldValue.CustomField.FieldName] = template.ReplaceVariables(fieldValue.Value, newUserName, newUser.SamAccountName);
                    }
                    catch (Exception ex)
                    {
                        Loggers.ActiveDirectoryLogger.Error("Could not set value for " + fieldValue.Field?.FieldName + ": " + fieldValue.Value?.ToString() + " {@Error}", ex);
                    }

                }
                if (newUserDetails.Fields != null)
                {
                    foreach (var field in newUserDetails.Fields)
                    {
                        var json = field.FieldValue as JsonElement?;
                        var kind = json.Value.ValueKind;
                        object? value=null;
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
                        newUser.SetCustomProperty(field.FieldName,value) ;
                    }
                }
                if (newUserDetails.Groups != null)
                {
                    foreach (var groupSid in newUserDetails.Groups)
                    {
                        var group = (IADGroup)Directory.GetDirectoryEntryByDN(groupSid);
                        if (group != null)
                        {
                            newUser.AssignTo(group);

                        }
                        else
                        {

                        }
                    }
                }
                

                IJob createUserJob = new Job(AppLocalization["Create User"]);
                createUserJob.StopOnFailedStep = true;
                //createUserJob.ShowJobDetailsDialog(MessageService);
                //_username = User.SamAccountName;
                //_userPassword = User.NewPassword;
                var result = await newUser.CommitChangesAsync(createUserJob);
                if (result.FailedSteps.Count == 0)
                {
                    newUser = (IADUser)Directory.GetDirectoryEntryByDN(newUser.DN);
                    await AuditLogger.User.Created(newUser);

                    _ = OUNotificationService.PostAsync(newUser, NotificationType.Create, CurrentUserState);

                    try
                    {
                        if (template?.EffectiveSendWelcomeEmail == true)
                        {
                            if (template.EffectiveAskForAlternateEmail == true || newUser.Email.IsNullOrEmpty())
                            {

                                await SendWelcomeEmail(newUser, newUserDetails.SendWelcomeEmailTo, password);

                            }
                            else
                            {
                                await SendWelcomeEmail(newUser, newUser.Email, password);
                            }
                        }
                    }
                    catch
                    {

                    }
                    return new CreatedResult(newUser.OU, newUser.DN);
                }
                else
                {
                    return new UnprocessableEntityObjectResult(result.FailedSteps.Select(s => s.Exception.InnerException != null ? s.Exception.InnerException.Message : s.Exception.Message));
                }
            }
            else
            {
                return new NotFoundObjectResult(templateId);
            }
            return new BadRequestResult();
        }


        /// <summary>
        /// Returns all user creation templates the user has access to.
        /// </summary>
        /// <response code="200">Returns a list of user creation templates.</response>
        /// <response code="401">Unauthorized - The user is not authenticated.</response>
        /// <response code="403">Forbidden - The user does not have the required role.</response>
        [HttpGet]
        [Route("/api/v1/templates/list/")]
        public IActionResult List()
        {
            using var context = DbFactory.CreateDbContext();
            var list = context.DirectoryTemplates.Where(t => t.DeletedAt == null && t.Visible).ToList();
            return FormatData(list);

        }

        private void Add(dynamic data, string title, string key)
        {
            var raw = User.Claims.FirstOrDefault(x => x.Type == key)?.Value;

            var str = raw?.ToString();
            data.Add(title, str);
        }
        private void AddDateTime(dynamic data, string title, string key)
        {
            var raw = User.Claims.FirstOrDefault(x => x.Type == key)?.Value;
            var lng = long.Parse(raw);


            var dt = DateTime.UnixEpoch.AddSeconds(lng); ;
            var str = dt.ToString();
            data.Add(title, str);
        }

        async Task SendWelcomeEmail(IADUser user, string to, SecureString password)
        {
            try
            {
                NewUserWelcomeEmailMessage message = new NewUserWelcomeEmailMessage();
                message.Domain = user.Directory.ConnectionSettings?.FQDN;
                message.Username = user.SamAccountName;
                message.Password = password;
                var html = message.Render();
                await EmailService.SendMessage(AppLocalization["New Account Details"], message, to);

            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error("Error sending welcome email {@Error}", ex);
            }
            //emailPreview = EmailService.PrepareHTMLForEmail(html);
        }
    }
}
