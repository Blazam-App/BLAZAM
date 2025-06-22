using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Exceptions;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Notifications;

using BLAZAM.FileSystem;
using BLAZAM.Gui.Helper;
using BLAZAM.Jobs;
using BLAZAM.Services.Events;
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
        bool homeDirectoryExists;
        bool showRemoveThumbnail = false;
        IGroupableDirectoryAdapter GroupableEntry => DirectoryEntry as IGroupableDirectoryAdapter;
        IAccountDirectoryAdapter Account => DirectoryEntry as IAccountDirectoryAdapter;
        IADUser User => DirectoryEntry as IADUser;
        IADContact Contact => DirectoryEntry as IADContact;

       

        private bool CanReadField(IActiveDirectoryField field)=> GroupableEntry.CanReadField(field);
        

        private bool CanEditField(IActiveDirectoryField field)=> GroupableEntry.CanEditField(field);
        






        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            await InvokeAsync(StateHasChanged);

          


            if (GroupableEntry is IADUser && User.HomeDirectory != null)
            {
                try
                {

                    await GroupableEntry.Directory.Impersonation.RunAsync(() =>

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
            if (Contact != null && !Contact.NewEntry)
            {
                ApplicationEvents.DirectoryEntryChanged.Invoke(new()
                {
                    EventType = ApplicationEventType.Search,
                    Entry = Contact,
                    Actor = CurrentUser.State

                });
            }
            LoadingData = false;
            await RefreshEntryComponents();

        }

        async Task SaveChanges()
        {
            if (await MessageService.Confirm(AppHelpLocalization[HelpLang.Confirm_Save_Changes]))
            {
                SavingChanges = true;
                await RefreshEntryComponents();
                try
                {

                    var changes = GroupableEntry.Changes;
                    var assignTo = GroupableEntry.ToAssignTo;
                    var unassignFrom = GroupableEntry.ToUnassignFrom;
                    var jobResults = await GroupableEntry.CommitChangesAsync();
                    if (jobResults.Result == JobResult.Passed)
                    {

                        foreach (var assignment in assignTo)
                        {
                            ApplicationEvents.DirectoryEntryChanged.Invoke(new()
                            {
                                EventType = ApplicationEventType.Assign,
                                Entry = assignment.Member,
                                Target = assignment.Group,
                                Actor = CurrentUser.State

                            });


                        }

                        foreach (var assignment in unassignFrom)
                        {
                            ApplicationEvents.DirectoryEntryChanged.Invoke(new()
                            {
                                EventType = ApplicationEventType.Unassign,
                                Entry = assignment.Member,
                                Target = assignment.Group,
                                Actor = CurrentUser.State

                            });

                        }
                        if (changes.Any(c => c.Field != ActiveDirectoryFields.MemberOf.FieldName))
                        {
                            ApplicationEvents.DirectoryEntryChanged.Invoke(new()
                            {
                                EventType = ApplicationEventType.Modify,
                                Entry = User,
                                Changes = changes.Where(c => c.Field != ActiveDirectoryFields.MemberOf.FieldName).ToList(),
                                Actor = CurrentUser.State

                            });

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
            if (await MessageService.Confirm("Are you sure you want to unlock " + GroupableEntry.DisplayName + "?", "Unlock User"))
            {
                Account.LockedOut = false;
                SnackBarService.Warning(GroupableEntry.DisplayName + " will be unlocked when changes are saved.");
                await RefreshEntryComponents();

            }

        }
        async Task DeleteUser()
        {
            if (await MessageService.Confirm("Are you sure you want to delete " + GroupableEntry.DisplayName + "?", "Delete User"))
            {
                SavingChanges = true;
                await InvokeAsync(StateHasChanged);

                try
                {
                    GroupableEntry.Delete();

                    SnackBarService.Success(GroupableEntry.DisplayName + " has been deleted.");
                    ApplicationEvents.DirectoryEntryChanged.Invoke(new()
                    {
                        EventType = ApplicationEventType.Delete,
                        Entry = User,
                        Actor = CurrentUser.State

                    });
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
            SnackBarService.Warning(GroupableEntry.DisplayName + " will have their thumbnail deleted on save.");
            await RefreshEntryComponents();

        }


    }
}