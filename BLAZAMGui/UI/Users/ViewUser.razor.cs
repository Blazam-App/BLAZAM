using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Exceptions;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.FileSystem;
using BLAZAM.Gui.Helper;
using BLAZAM.Jobs;
using BLAZAM.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BLAZAM.Gui.UI.Users
{
    public partial class ViewUser: DirectoryEntryViewBase
    {
#nullable disable warnings
        string password;

        [Parameter]
        public string Password
        {
            get => password; set
            {
                if (password == value) return;
                password = value;
                PasswordChanged.InvokeAsync(password);
            }
        }

        [Parameter]
        public EventCallback<string> PasswordChanged { get; set; }


        string confirmPassword;
        [Parameter]
        public string ConfirmPassword
        {
            get => confirmPassword; set
            {
                if (confirmPassword == value) return;
                confirmPassword = value;
                ConfirmPasswordChanged.InvokeAsync(confirmPassword);
            }
        }
        [Parameter]
        public EventCallback<string> ConfirmPasswordChanged { get; set; }
        AccessLevel? selfAccessLevel;
        bool homeDirectoryExists;
        bool showRemoveThumbnail = false;
        IAccountDirectoryAdapter Account => DirectoryEntry as IAccountDirectoryAdapter;
        IADUser User => DirectoryEntry as IADUser;
        IADContact Contact => DirectoryEntry as IADContact;
        GlobalPermissionSettings? _globalPermissionSettings;

        private bool isSelf
        {
            get
            {
                if (_globalPermissionSettings != null)
                {
                    if (selfAccessLevel != null)
                    {
                        return Account.SamAccountName.Equals(CurrentUser.Username, StringComparison.InvariantCultureIgnoreCase);
                    }
                }
                return false;
            }
        }


        private bool CanReadField(IActiveDirectoryField field)
        {
            if (!isSelf)
            {
                return Account.CanReadField(field);
            }
            else
            {
                if (selfAccessLevel.FieldMap.Any(x => x.Field.Id == field.Id && x.FieldAccessLevel.Level > FieldAccessLevels.Deny.Level))
                {
                    return true;
                }
                else if (selfAccessLevel.FieldMap.Any(x => x.CustomField.Id == field.Id && x.FieldAccessLevel.Level > FieldAccessLevels.Deny.Level))
                {
                    return true;
                }
                else
                {
                    return Account.CanReadField(field);
                }
            }
        }

        private bool CanEditField(IActiveDirectoryField field)
        {
            if (!isSelf)
            {
                return Account.CanEditField(field);
            }
            else
            {
                if (selfAccessLevel.FieldMap.Any(x => x.Field.Id == field.Id && x.FieldAccessLevel.Level > FieldAccessLevels.Read.Level))
                {
                    return true;
                }
                else if (selfAccessLevel.FieldMap.Any(x => x.CustomField.Id == field.Id && x.FieldAccessLevel.Level > FieldAccessLevels.Read.Level))
                {
                    return true;
                }
                else
                {
                    return Account.CanEditField(field);
                }
            }
        }






        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            await InvokeAsync(StateHasChanged);

            if (Context != null)
            {
                _globalPermissionSettings = await Context.GlobalPermissionSettings.FirstOrDefaultAsync();
                selfAccessLevel = await Context.AccessLevels.FirstOrDefaultAsync(x => x.Name == AccessLevel.SelfAccessLevelName);
            }


            if (Account is IADUser && User.HomeDirectory != null)
            {
                try
                {

                    await Account.Directory.Impersonation.RunAsync(() =>

                    {
                        homeDirectoryExists = new SystemDirectory(User.HomeDirectory).Exists;
                        return true;
                    });

                }

                catch (Exception ex)
                {
                    Loggers.ActiveDirectoryLogger.Warning("Error checking user h-drive: {Message}", ex);

                }
            }
            await AuditLogger.Searched(Account);
            LoadingData = false;
            await RefreshEntryComponents();

        }

        async void SaveChanges()
        {
            if (await MessageService.Confirm(AppHelpLocalization[HelpLang.Confirm_Save_Changes]))
            {
                SavingChanges = true;
                await RefreshEntryComponents();
                try
                {

                    var changes = Account.Changes;
                    var assignTo = Account.ToAssignTo;
                    var unassignFrom = Account.ToUnassignFrom;
                    var jobResults = await Account.CommitChangesAsync();
                    if (jobResults.Result == JobResult.Passed)
                    {

                        foreach (var assignment in assignTo)
                        {
                            await AuditLogger.User.Assigned(assignment.Member, assignment.Group);
                            await AuditLogger.Group.MemberAdded(assignment.Group, assignment.Member);
                            //Run synchronously if using sqlite
                            if (DbFactory.DatabaseType == DatabaseType.SQLite)
                            {
                                await NotificationGenerationService.PostAsync(Account, NotificationType.Assign, CurrentUser.State, assignment.Group);

                            }
                            else
                            {
                                _ = NotificationGenerationService.PostAsync(Account, NotificationType.Assign, CurrentUser.State, assignment.Group);

                            }

                        }

                        foreach (var assignment in unassignFrom)
                        {
                            await AuditLogger.User.Unassigned(assignment.Member, assignment.Group);
                            await AuditLogger.Group.MemberRemoved(assignment.Group, assignment.Member);
                            //Run synchronously if using sqlite
                            if (DbFactory.DatabaseType == DatabaseType.SQLite)
                            {
                                await NotificationGenerationService.PostAsync(Account, NotificationType.Unassign, CurrentUser.State, assignment.Group);

                            }
                            else
                            {
                                _ = NotificationGenerationService.PostAsync(Account, NotificationType.Unassign, CurrentUser.State, assignment.Group);

                            }

                        }
                        if (changes.Any(c => c.Field != ActiveDirectoryFields.MemberOf.FieldName))
                        {
                            await AuditLogger.User.Changed(Account, changes.Where(c => c.Field != ActiveDirectoryFields.MemberOf.FieldName).ToList());

                            //Run synchronously if using sqlite
                            if (DbFactory.DatabaseType == DatabaseType.SQLite)
                            {
                                await NotificationGenerationService.PostAsync(Account, NotificationType.Modify, CurrentUser.State);

                            }
                            else
                            {
                                _ = NotificationGenerationService.PostAsync(Account, NotificationType.Modify, CurrentUser.State);

                            }
                        }
                        EditMode = false;
                        SnackBarService.Success("The changes made to this user have been saved.");
                    }
                    else
                    {
                        await jobResults.ShowJobDetailsDialogAsync(MessageService);
                    }
                }
                catch (AppException ex)
                {
                    SnackBarService.Error(ex.Message);

                }
                SavingChanges = false;

                await RefreshEntryComponents();

            }

        }

        async Task Unlock()
        {
            if (await MessageService.Confirm("Are you sure you want to unlock " + Account.DisplayName + "?", "Unlock User"))
            {
                Account.LockedOut = false;
                SnackBarService.Warning(Account.DisplayName + " will be unlocked when changes are saved.");
                await RefreshEntryComponents();

            }

        }
        async Task DeleteUser()
        {
            if (await MessageService.Confirm("Are you sure you want to delete " + Account.DisplayName + "?", "Delete User"))
            {
                SavingChanges = true;
                await InvokeAsync(StateHasChanged);

                try
                {
                    Account.Delete();

                    SnackBarService.Success(Account.DisplayName + " has been deleted.");
                    await AuditLogger.User.Deleted(Account);
                    _ = NotificationGenerationService.PostAsync(Account, NotificationType.Delete, CurrentUser.State);

                }
                catch (AppException ex)
                {
                    SnackBarService.Error(ex.Message);
                }
                SavingChanges = false;

                await RefreshEntryComponents();
            }
        }
        async Task RemoveThumbnail()
        {

            Contact.ThumbnailPhoto = null;
            SnackBarService.Warning(Account.DisplayName + " will have their thumbnail deleted on save.");
            await RefreshEntryComponents();

        }


    }
}