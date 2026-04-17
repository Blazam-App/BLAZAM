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

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            Modal.YesEnabled = (() => {return IsValid; });
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            await ValidateModalAsync();
        }

        protected new MudForm? Form { get; set; }

        protected new virtual bool IsValid { get; set; } = true;
        private bool _lastIsValid;
        private async Task<bool> ValidateModalAsync()
        {
            if (Form != null)
            {
                await Form.ValidateAsync();
            }

            if (_lastIsValid != IsValid)
            {
                _lastIsValid = IsValid;
                Modal?.RefreshView();
            }
            return IsValid;
        }


    }
}