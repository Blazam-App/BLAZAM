using MudBlazor;

namespace BLAZAM.Gui.UI.Inputs.TreeViews
{
    public partial class ADTreeView : OUTreeViewBase
    {
        /// <summary>
        /// If true all directory entry types will be listed.
        /// If false, only OU's will be shown.
        /// </summary>
        /// <remarks>
        /// Default value is true
        /// </remarks>
        [Parameter]
        public bool ShowAllEntries { get; set; } = true;






        protected override IReadOnlyCollection<TreeItemData<IDirectoryEntryAdapter>> GetItems(IDirectoryEntryAdapter? parent)
        {
            try
            {
                if (parent?.IsExpanded == true || parent?.CachedChildren != null)
                {
                    return GetChildren(parent).ToTreeItemData();
                }
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Information(ex, "Error getting children for {@Parent}", parent);

            }
            return [];

        }
        protected async Task<IReadOnlyCollection<TreeItemData<IDirectoryEntryAdapter>>> GetChildrenAsync(IDirectoryEntryAdapter parentNode)
        {
            return await Task.Run(() =>
            {
                return GetChildren(parentNode).ToTreeItemData();

            });
        }
        private Dictionary<IDirectoryEntryAdapter, IEnumerable<IDirectoryEntryAdapter>?> _childrenCache = new();
        protected IEnumerable<IDirectoryEntryAdapter> GetChildren(IDirectoryEntryAdapter parentNode)
        {

            if (!_childrenCache.ContainsKey(parentNode))
            {


                if (ShowAllEntries)
                {
                    var children = parentNode.Children
                        .Where(c => (c.ObjectType == ActiveDirectoryObjectType.OU && ShouldShowOU(c)) || (c.CanRead))
                        .MoveToTop(c => c.ObjectType == ActiveDirectoryObjectType.Group)
                        .MoveToTop(c => c.ObjectType == ActiveDirectoryObjectType.OU); ;
                    _childrenCache[parentNode] = children;

                }
                else if (parentNode is IADOrganizationalUnit ou)
                {

                    var children = ou.Children.Where(c => c.ObjectType == ActiveDirectoryObjectType.OU);
                    _childrenCache[parentNode] = children;
                }
                else
                {
                    _childrenCache[parentNode] = new List<IDirectoryEntryAdapter>();
                }
            }
            return _childrenCache[parentNode] ?? new List<IDirectoryEntryAdapter>();

        }
    }
}