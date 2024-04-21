using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using Color = MudBlazor.Color;

namespace BLAZAM.Gui.UI.Inputs.TreeViews
{
    public partial class ADTreeView : OUTreeViewBase
    {
#nullable disable warnings
        /// <summary>
        /// If true all directory entry types will be listed.
        /// If false, only OU's will be shown.
        /// </summary>
        /// <remarks>
        /// Default value is true
        /// </remarks>
        [Parameter]
        public bool ShowAllEntries { get; set; } = true;


      
      
       

        private HashSet<IDirectoryEntryAdapter>? GetItems(IDirectoryEntryAdapter parent)
        {
            if (parent.IsExpanded || parent.CachedChildren != null)
            {

                var items = parent.Children
                    .Where(c => (c.ObjectType == ActiveDirectoryObjectType.OU && ShouldShowOU(c)) || c.CanRead)
                    .MoveToTop(c => c.ObjectType == ActiveDirectoryObjectType.OU);
                if (!ShowAllEntries)
                {
                    items = items.Where(i => i.ObjectType == ActiveDirectoryObjectType.OU);
                }
                var hashst = items.ToHashSet();
                return hashst;
            }
            return null;
        }
        protected async Task<HashSet<IDirectoryEntryAdapter>> GetChildrenAsync(IDirectoryEntryAdapter parentNode)
        {
            return await Task.Run(() =>
            {
                return GetChildren(parentNode);

            });
        }
        protected HashSet<IDirectoryEntryAdapter> GetChildren(IDirectoryEntryAdapter parentNode)
        {

            if (ShowAllEntries)
            {
                var children = parentNode.Children.ToHashSet();
                return children;

            }
            else if (parentNode is IADOrganizationalUnit ou)
            {

                var children = ou.Children.Where(c => c.ObjectType == ActiveDirectoryObjectType.OU).ToHashSet();
                return children;
            }
            return new();


        }
    }
}