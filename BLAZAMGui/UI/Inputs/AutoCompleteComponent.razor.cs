

namespace BLAZAM.Gui.UI.Inputs
{
    public class AutoCompleteComponentBase:AppComponentBase
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

        string searchTerm;
    }
}