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

        HashSet<IDirectoryEntryAdapter> GetChildren(IDirectoryEntryAdapter context)
        {
            if (context is IADOrganizationalUnit ou)
            {
                if (ou.IsExpanded)
                {
                    return ou.TreeViewSubOUs.Where(o=>ShouldShowOU(o)).ToHashSet();
                }
                else
                {
                    return ou.CachedTreeViewSubOUs.Where(o => ShouldShowOU(o)).ToHashSet();
                }
            }
            return new HashSet<IDirectoryEntryAdapter>();
        }
        async Task<HashSet<IDirectoryEntryAdapter>> GetChildrenAsync(IDirectoryEntryAdapter parentNode)
        {
            return await Task.Run(() =>
                {
                    return GetChildren(parentNode);


                });
        }




















    }
}