using MudBlazor;

namespace BLAZAM.Gui.UI
{
    public abstract class AppModalContent : DirectoryModelComponent
    {

        [CascadingParameter] protected AppModal Modal { get; set; }
        [Parameter]
        public EventCallback ModelChanged { get; set; }
        /// <summary>
        /// Closes the containing modal
        /// </summary>
        protected void Close()
        {
            Modal.Close();
            _ = StateHasChangedAsync();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Modal.YesEnabled = ValidateModal;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            ValidateModal();
        }

        protected new MudForm? Form { get; set; }

        protected new virtual bool IsValid { get; set; } = true;
        private bool _lastIsValid;
        private bool ValidateModal()
        {
            Form?.Validate();

            if (_lastIsValid != IsValid)
            {
                _lastIsValid = IsValid;
                Modal?.RefreshView();
            }
            return IsValid;
        }


    }
}