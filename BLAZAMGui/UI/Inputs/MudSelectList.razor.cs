
using MudBlazor;

namespace BLAZAM.Gui.UI.Inputs
{
    public partial class MudSelectList<T> : MudSelect<T>
    {
        

        [Parameter]
        public IEnumerable<T> Values { get; set; }
    }
}