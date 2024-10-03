

namespace BLAZAM.Gui.UI
{
    public class TabbedAppComponentBase : DatabaseComponentBase
    {
        /// <summary>
        /// The base tab uri
        /// </summary>
        /// <remarks>
        /// eg: /settings
        /// </remarks>
        protected string BaseUri = "/settings";

        [Parameter]
        public int ActiveTab { get; set; } = 0;

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }
        protected Task OnSelectedTabChanged(int index)
        {
            ActiveTab = index;
            Nav.NavigateTo(BaseUri + "/" + index);
            return Task.CompletedTask;
        }
    }
}