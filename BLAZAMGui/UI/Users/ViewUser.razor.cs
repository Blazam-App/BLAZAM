using BLAZAM.Gui.Helper;
using BLAZAM.Jobs;
using BLAZAM.Services.Events;
using MudBlazor;

namespace BLAZAM.Gui.UI.Users
{
    public partial class ViewUser : DirectoryEntryViewBase
    {
        private string _password;

        [Parameter]
        public string Password
        {
            get => _password; set
            {
                if (_password == value)
                {
                    return;
                }

                _password = value;
                PasswordChanged.InvokeAsync(_password);
            }
        }

        [Parameter]
        public EventCallback<string> PasswordChanged { get; set; }


        private string _confirmPassword;
        [Parameter]
        public string ConfirmPassword
        {
            get => _confirmPassword; set
            {
                if (_confirmPassword == value)
                {
                    return;
                }

                _confirmPassword = value;
                ConfirmPasswordChanged.InvokeAsync(_confirmPassword);
            }
        }
        [Parameter]
        public EventCallback<string> ConfirmPasswordChanged { get; set; }

        private IGroupableDirectoryAdapter? GroupableEntry => DirectoryEntry as IGroupableDirectoryAdapter;

        private IAccountDirectoryAdapter? Account => DirectoryEntry as IAccountDirectoryAdapter;

        private IADUser? User => DirectoryEntry as IADUser;

        private IADContact? Contact => DirectoryEntry as IADContact;



        private bool CanReadField(IActiveDirectoryField field) => GroupableEntry?.CanReadField(field) ?? false;


        private bool CanEditField(IActiveDirectoryField field) => GroupableEntry?.CanEditField(field) ?? false;






        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            await StateHasChangedAsync();



            if (Contact != null)
            {
                ActiveDirectoryEvents.DirectoryEntryEvent.Invoke(new()
                {

                    EventType = ApplicationEventType.Search,
                    Entry = Contact,
                    Actor = CurrentUser.State

                });
            }

            LoadingData = false;
            await RefreshEntryComponents();

        }

        private async Task SaveChanges()
        {
            if (await MessageService.Confirm(AppHelpLocalization[HelpLang.Confirm_Save_Changes]))
            {
                SavingChanges = true;
                await RefreshEntryComponents();
                try
                {
                    if (GroupableEntry == null)
                    {
                        SnackBarService.Error("Unable to save changes. No entry loaded.");
                        return;
                    }
                    var changes = GroupableEntry.Changes;
                    var assignTo = GroupableEntry.ToAssignTo;
                    var unassignFrom = GroupableEntry.ToUnassignFrom;
                    var jobResults = await GroupableEntry.CommitChangesAsync();
                    if (jobResults.Result == JobResult.Passed)
                    {

                        foreach (var assignment in assignTo)
                        {
                            ActiveDirectoryEvents.DirectoryEntryEvent.Invoke(new()
                            {
                                EventType = ApplicationEventType.Assign,
                                Entry = assignment.Member,
                                Target = assignment.Group,
                                Actor = CurrentUser.State

                            });


                        }

                        foreach (var assignment in unassignFrom)
                        {
                            ActiveDirectoryEvents.DirectoryEntryEvent.Invoke(new()
                            {
                                EventType = ApplicationEventType.Unassign,
                                Entry = assignment.Member,
                                Target = assignment.Group,
                                Actor = CurrentUser.State

                            });

                        }
                        if (changes.Any(c => c.Field != ActiveDirectoryFields.MemberOf.FieldName)
                            && User != null)
                        {
                            ActiveDirectoryEvents.DirectoryEntryEvent.Invoke(new()
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

        private async Task Unlock()
        {
            if (await MessageService.Confirm("Are you sure you want to unlock " + GroupableEntry?.DisplayName + "?", "Unlock User"))
            {
                Account!.LockedOut = false;
                SnackBarService.Warning(GroupableEntry?.DisplayName + " will be unlocked when changes are saved.");
                await RefreshEntryComponents();

            }

        }
        private async Task DeleteUser()
        {
            if (await MessageService.Confirm("Are you sure you want to delete " + GroupableEntry?.DisplayName + "?", "Delete User"))
            {
                SavingChanges = true;
                await StateHasChangedAsync();

                try
                {
                    GroupableEntry?.Delete();

                    SnackBarService.Success(GroupableEntry?.DisplayName + " has been deleted.");
                    if (User != null)
                    {
                        ActiveDirectoryEvents.DirectoryEntryEvent.Invoke(new()
                        {
                            EventType = ApplicationEventType.Delete,
                            Entry = User,
                            Actor = CurrentUser.State

                        });
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
        private bool _disposed = false;

        public override void Dispose()
        {
            if (_disposed)
                return;

            // Dispose managed resources here
            _confirmPassword = String.Empty;
            _password = String.Empty;
            if (GroupableEntry is IDisposable disposableGroupable)
            {
                disposableGroupable.Dispose();
            }

            base.Dispose();
            _disposed = true;
        }
    }
}