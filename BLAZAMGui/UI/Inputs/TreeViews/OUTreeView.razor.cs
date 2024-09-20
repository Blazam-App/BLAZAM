using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BLAZAM.Gui.UI.Inputs.TreeViews
{
    public partial class OUTreeView : OUTreeViewBase
    {
#nullable disable warnings




        [Parameter]
        public RenderFragment<IADOrganizationalUnit>? EndAdornment { get; set; }






















    }
}