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






        private IReadOnlyCollection<TreeItemData<IDirectoryEntryAdapter>>? GetItems(IDirectoryEntryAdapter? parent)
        {
            try
            {
                if (parent.IsExpanded || parent.CachedChildren != null)
                {
                    return GetChildren(parent).ToTreeItemData();
                }
            }
            catch (Exception)
            {
                return null;

            }
            return null;

        }
        protected async Task<IReadOnlyCollection<TreeItemData<IDirectoryEntryAdapter>>> GetChildrenAsync(IDirectoryEntryAdapter parentNode)
        {
            return await Task.Run(() =>
            {
                return GetChildren(parentNode).ToTreeItemData();

            });
        }
        protected IEnumerable<IDirectoryEntryAdapter> GetChildren(IDirectoryEntryAdapter parentNode)
        {

            if (ShowAllEntries)
            {
                var children = parentNode.Children
                    .Where(c => (c.ObjectType == ActiveDirectoryObjectType.OU && ShouldShowOU(c)) || c.CanRead)
                    .MoveToTop(c => c.ObjectType == ActiveDirectoryObjectType.Group)
                    .MoveToTop(c => c.ObjectType == ActiveDirectoryObjectType.OU); ;
                return children;

            }
            else if (parentNode is IADOrganizationalUnit ou)
            {

                var children = ou.Children.Where(c => c.ObjectType == ActiveDirectoryObjectType.OU);
                return children;
            }
            return new List<IDirectoryEntryAdapter>();


        }
    }
}