
using Microsoft.AspNetCore.Components;
using BLAZAM.Server.Data.Services;
using BLAZAM.Common.Data.Database;
using MudBlazor;

namespace BLAZAM.Server.Shared.UI
{

    public delegate void OnYesEvent();
    public delegate void OnCancelEvent();

    public partial class AppModal
    {
#nullable disable warnings
        [Inject]
        protected AppSnackBarService NotificationService { get; set; }

        /// <summary>
        /// The modal's  databse connection
        /// </summary>
        [Parameter]
        public IDatabaseContext? Context { get; set; }

        /// <summary>
        /// If set to false, will prevent this modal from closing via the UI
        /// </summary>
        [Parameter]
        public bool AllowClose { get; set; } = true;
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

        //[Parameter]
        //public EventCallback OnNo { get; set; }
        [Parameter]
        public OnCancelEvent OnCancel { get; set; }

        [Parameter]
        public OnYesEvent OnYes { get; set; }

        [Parameter]
        public string YesText { get; set; } = "Ok";
        [Parameter]
        public string CancelText { get; set; }




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
            get =>  Modal.IsVisible;
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
            return Modal?.Show();
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

    }
}