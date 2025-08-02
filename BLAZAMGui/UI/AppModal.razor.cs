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
        public Color CancelColor { get; set; }
        public void SetCancelColor(Color cancelColor)
        {
            CancelColor = cancelColor;
        }
        [Parameter]
        public OnCancelEvent OnCancel { get; set; }
        public void SetOnCancel(OnCancelEvent onCancel)
        {
            OnCancel = onCancel;
        }
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
            if (YesText.IsNullOrEmpty())
            {
                YesText = AppLocalization[Lang.Ok];
            }
            if (Options == null)
            {
                Options = new()
                {
                    MaxWidth = Width,
                    CloseButton = true
                };
            }

            //AllowClose = true;
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
            Modal.CloseAsync(); //Fix for MudBlazor Bug causing modal to no reopen after one click but two, suggesting a state sync issue, remove if fixed
           var @ref = await Modal.ShowAsync(null, Options);
             await InvokeAsync(StateHasChanged);
            return @ref;
        }


        /// <summary>
        /// Hide this modal
        /// </summary>
        public void Close()
        {
            Modal.CloseAsync();
            IsShown = false;
        }
        /// <summary>
        /// Hide this modal
        /// </summary>
        public async Task CloseAsync()
        {
            await Modal.CloseAsync();
            IsShown = false;

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