
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BLAZAM.Server.Shared.UI.Inputs
{
    public partial class MudSelectList<T> : MudSelect<T>
    {
        

        [Parameter]
        public IEnumerable<T> Values { get; set; }
    }
}