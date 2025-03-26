using MudBlazor;

namespace BLAZAM.Gui.UI
{

    public delegate Task OnYesEvent();
    public delegate Task OnCancelEvent();

    public partial class AppModal
    {
#nullable disable warnings
        [Inject]
        protected AppSnackBarService NotificationService { get; set; }
        [Inject]
        protected IStringLocalizer<AppLocalization> AppLocalization { get; set; }
        [Inject]
        protected IStringLocalizer<AppHelpLocalization> AppHelpLocalization { get; set; }
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
                    Options = new()
                    {
                        BackdropClick = value,
                        CloseButton = value,
                        CloseOnEscapeKey = value
                    };

                RefreshView();
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
        protected MudDialog Modal { get; set; }


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

        [Parameter]
        public OnCancelEvent OnCancel { get; set; }

        [Parameter]
        public OnYesEvent OnYes { get; set; }
        public void SetOnYes(OnYesEvent onYes)
        {
            OnYes = onYes;
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
            get => Modal.Visible;
            set
            {
                if (value == Modal.Visible)
                    return;
                Modal.Visible = value;
                IsShownChanged.InvokeAsync(value);
            }
        }

        [Parameter]
        public EventCallback<bool> IsShownChanged { get; set; }
        [Parameter]
        public MaxWidth? Width { get; set; }

       
        protected override void OnInitialized()
        {
            base.OnInitialized();
            YesText = AppLocalization[Lang.Ok];
            if (Options == null)
                Options = new()
                {
                    MaxWidth = Width
                };

            AllowClose = true;
        }
        /// <summary>
        /// Re-renders the modal with the latest property values
        /// </summary>
        public async Task RefreshView()
        {
            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Show this modal
        /// </summary>
        public async Task<IDialogReference?> ShowAsync()
        {

            IsShown = true;

            return await Modal.ShowAsync(null, Options);
        }


        /// <summary>
        /// Hide this modal
        /// </summary>
        public void Close()
        {
            IsShown = false;
            Modal.CloseAsync();
        }
        private void YesClicked()
        {
            if (OnYes != null)
                OnYes?.Invoke();
            else
                Close();
        }
       
    }
}