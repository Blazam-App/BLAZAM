using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BLAZAM.Gui.UI.Inputs.TreeViews
{
    public partial class OUTreeView:OUTreeViewBase
    {
#nullable disable warnings




        [Parameter]
        public RenderFragment<IADOrganizationalUnit>? EndAdornment { get; set; }

        List<TreeItemData<IDirectoryEntryAdapter>> GetChildren(IDirectoryEntryAdapter context)
        {
            if (context is IADOrganizationalUnit ou)
            {
                if (ou.IsExpanded)
                {
                    return ou.TreeViewSubOUs.Where(o=>ShouldShowOU(o)).ToTreeItemData();
                }
                else
                {
                    return ou.CachedTreeViewSubOUs.Where(o => ShouldShowOU(o)).ToTreeItemData();
                }
            }
            return new List<TreeItemData<IDirectoryEntryAdapter>>();
        }
        async Task<IReadOnlyCollection<TreeItemData<IDirectoryEntryAdapter>>> GetChildrenAsync(IDirectoryEntryAdapter parentNode)
        {
            return await Task.Run(() =>
                {
                    return GetChildren(parentNode);


                });
        }




















    }
}