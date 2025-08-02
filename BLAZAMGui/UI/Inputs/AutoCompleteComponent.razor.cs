

namespace BLAZAM.Gui.UI.Inputs
{
    public class AutoCompleteComponentBase : AppComponentBase
    {

        [Parameter]
        public string SearchTerm
        {
            get => searchTerm;
            set
            {
                if (searchTerm == value)
                    return;
                searchTerm = value;
                SearchTermChanged.InvokeAsync(value);
                InvokeAsync(StateHasChanged);
            }
        }

        [Parameter]
        public EventCallback<string> SearchTermChanged { get; set; }

        [Parameter]
        public int MinCharacters { get; set; } = 3;

        private string searchTerm;
    }
}