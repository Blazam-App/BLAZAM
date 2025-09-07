
using BLAZAM.Global.Data;
using BLAZAM.Gui.Helpers;

namespace BLAZAM.Gui.UI.Users
{
    public partial class CreateUser : TemplateComponent
    {
        private bool _userCreated = false;
        private string customConfirmPassword;
        private string customPassword;
        private bool _usernameStepValid = false;
        private int _templateCarouselIndex = 0;
        private int selectedStep;
        private int SelectedStep
        {
            get => selectedStep; set

            {
                if (selectedStep == value) return;
                selectedStep = value;
                _ = StateHasChangedAsync();

            }
        }
        private AppModal? _assignToModal;

        protected Task OnSelectedStepChanged(int name)
        {
            SelectedStep = name;

            return Task.CompletedTask;
        }


        private string? customUserDisplayName;
        private bool? custom = false;

        private NewUserName newUserName = new NewUserName();
        private IADOrganizationalUnit? selectedOU;
        private IADUser? newUser;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await StateHasChangedAsync();
        }

        private async Task OUSelected(IDirectoryEntryAdapter entry)
        {
            if (entry is IADOrganizationalUnit ou)
            {
                if (ou.CanCreateUser)
                    selectedOU = ou;
                else
                {
                    SnackBarService.Warning(AppLocalization["You do not have permission to create users in that location"]);
                }
            }

            await StateHasChangedAsync();
        }
        private async Task CreateCustomUser()
        {
            await Task.Run(() =>
            {
                if (selectedOU != null)
                {
                    if (newUser == null)
                    {
                        if (customUserDisplayName == null) SnackBarService.Error(AppLocalization["No display name was set"]);
                        else
                        {
                            newUser = selectedOU.CreateUser(customUserDisplayName.Trim());
                            newUser.DisplayName = customUserDisplayName;
                        }
                    }
                    else
                    {
                        newUser.MoveTo(selectedOU);
                    }
                    _userCreated = false;
                }
            });

        }
        private async Task CreateTemplateUser()
        {
            if (SelectedTemplate?.ParentOU == null) throw new AppException("Parent OU for template user was not set on creation!");
            try
            {
                LoadingData = true;
                newUser = SelectedTemplate.GenerateTemplateUser(newUserName, Directory);
                _userCreated = false;

                if (IsAdmin || SelectedTemplate.HasEditableFields())
                {
                    SelectedStep = 2;
                }
                else
                {
                    //Go to confirm step
                    SelectedStep = 7;

                }

            }
            catch (Exception ex)
            {

                Loggers.ActiveDirectoryLogger.Error(ex, "Error while creating template user");

                SnackBarService.Error(AppLocalization["An error has occurred while trying to create the template user: "] + ex.Message);
            }
            LoadingData = false;
        }

        private async Task SetTemplate(DirectoryTemplate selectedTemplate)
        {
            SelectedTemplate = selectedTemplate;
            custom = false;
            SelectedStep = 1;
            await StateHasChangedAsync();
        }
        private bool AdditionalShow(IDirectoryEntryAdapter entry)
        {
            if (entry is IADOrganizationalUnit ou)
            {
                if (ou.CanCreateUser) return true;
                if (ou.CanCreateUserInSubOUs) return true;
            }
            return false;
        }
        private async Task UserConfirmed()
        {
            newUserName = new();
            _userCreated = true;
            await StateHasChangedAsync();
        }
    }
}