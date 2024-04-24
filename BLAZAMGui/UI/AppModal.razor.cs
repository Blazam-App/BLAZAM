
using Microsoft.AspNetCore.Components;
using BLAZAM.Server.Data.Services;
using MudBlazor;
using BLAZAM.Database.Context;
using BLAZAM.Notifications.Services;

namespace BLAZAM.Gui.UI
{

    public delegate void OnYesEvent();
    public delegate void OnCancelEvent();

    public partial class AppModal
    {
#nullable disable warnings
        [Inject]
        protected AppSnackBarService NotificationService { get; set; }
        [Inject]
        protected IStringLocalizer<AppLocalization> AppLocalization { get; set; }
        /// <summary>
        /// The modal's  database connection
        /// </summary>
        [Parameter]
        public IDatabaseContext? Context { get; set; }

        /// <summary>
        /// If set to false, will prevent this modal from closing via the UI
        /// </summary>
        [Parameter]
        public bool AllowClose
        {
            get => Options.CloseButton == true; set
            {
                if (Options == null)
                    Options = new();
                Options.DisableBackdropClick = !value;
                Options.CloseButton = value;
                Options.CloseOnEscapeKey = value;
            }
        }
        [Parameter]
        public Color Color { get; set; } = Color.Default;
        /// <summary>
        /// The modal content. By default, there is no content
        /// </summary>
        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// A reference to this modal
        /// </summary>
        protected MudDialog? Modal { get; set; }


        private bool loadingData = false;
        [Parameter]
        public bool LoadingData
        {
            get => loadingData; set
            {
                loadingData = value;
                InvokeAsync(StateHasChanged);
            }
        }
        //[Parameter]
        //public EventCallback OnNo { get; set; }
        [Parameter]
        public OnCancelEvent OnCancel { get; set; }

        [Parameter]
        public OnYesEvent OnYes { get; set; }
        public void SetOnYes(OnYesEvent onYes)
        {
            OnYes=onYes;
        }
        [Parameter]
        public string YesText { get; set; }
        public void SetYesText(string text)
        {
            YesText = text;
        }
        [Parameter]
        public string CancelText { get; set; }
        public void SetCancelText(string text)
        {
            CancelText = text;
        }




        public Func<bool> YesEnabled { get; set; } = (() => { return true; });



        [Parameter]
        public string Title { get; set; }

        [Parameter]
        public EventCallback<MudMessageBox>? ModalChanged { get; set; }

        /// <summary>
        /// Indicates whether this modal is currently shown
        /// </summary>
        [Parameter]
        public bool IsShown
        {
            get => Modal.IsVisible;
            set
            {
                if (value == Modal.IsVisible)
                    return;
                Modal.IsVisible = value;
                IsShownChanged.InvokeAsync(value);
            }
        }

        [Parameter]
        public EventCallback<bool> IsShownChanged { get; set; }


        protected override void OnInitialized()
        {
            base.OnInitialized();
            YesText = AppLocalization["Ok"]; 
            if (Options == null)
                Options = new();
            AllowClose=true;
        }
        /// <summary>
        /// Re-renders the modal with the latest property values
        /// </summary>
        public void RefreshView()
        {
            InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Show this modal
        /// </summary>
        public IDialogReference? Show()
        {

            IsShown = true;

            return Modal?.Show(null,Options);
        }

        /// <summary>
        /// Hide this modal
        /// </summary>
        public void Hide()
        {
            IsShown = false;
            Modal?.Close();
        }
        private void YesClicked()
        {
            if (OnYes != null)
                OnYes?.Invoke();
            else
                Hide();
        }
        /// <summary>
        /// Sets the modal to be fullscreen, disabled by passing false.
        /// </summary>
        /// <param name="enabled"></param>
        public void Fullscreen(bool enabled = true)
        {
            var existingOptions = Modal.Options;
            existingOptions.FullScreen = enabled;
            existingOptions.FullWidth = enabled;
            if (enabled)
            {
                existingOptions.MaxWidth = MaxWidth.ExtraExtraLarge;
            }
            Modal.Options = existingOptions;
            RefreshView();
        }
    }
}