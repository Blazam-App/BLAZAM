using MudBlazor;
using Color = MudBlazor.Color;

namespace BLAZAM.Gui.UI.Inputs.TreeViews
{
    public class OUTreeViewBase : AppComponentBase
    {

        protected ADOrganizationalUnit TopLevel;
        private IADOrganizationalUnit? _startingSelectedNode;
        private IDirectoryEntryAdapter? _selectedEntry;
        protected MudTreeView<IDirectoryEntryAdapter>? treeView;
        [Parameter]
        public bool StartRootExpanded { get; set; } = true;

        [Parameter]
        public string? Label { get; set; }
        /// <summary>
        /// The root ou of this TreeView
        /// </summary>
        /// <remarks>
        /// Defaults to the App Base root
        /// </remarks>
        [Parameter]
        public IReadOnlyCollection<TreeItemData<IDirectoryEntryAdapter>>? RootOU { get; set; } = [];

        [Parameter]
        public IADOrganizationalUnit? StartingSelectedOU
        {
            get => _startingSelectedNode; set
            {
                if (value == _startingSelectedNode)
                {
                    return;
                }

                _startingSelectedNode = value;
                SelectedEntry = value;


                StartingSelectedOUChanged.InvokeAsync(value);
            }

        }

        [Parameter]
        public EventCallback<IADOrganizationalUnit> StartingSelectedOUChanged
        {
            get; set;
        }

        [Parameter]
        public IDirectoryEntryAdapter? SelectedEntry
        {
            get => _selectedEntry; set
            {
                if (value == _selectedEntry)
                {
                    return;
                }

                if (value != null)
                {
                    var cache = _selectedEntry;

                    _selectedEntry = value;
                    if (cache == null && RootOU?.Count > 0 && value == RootOU.First())
                    {
                        return;
                    }

                    InvokeAsync(() => { SelectedEntryChanged.InvokeAsync(value); });

                }
            }

        }
        protected Color GetIconColor(ITreeItemData<IDirectoryEntryAdapter> context)
        {
            return context.Selected == true ? Color.Primary : Color.Default;
        }
        [Parameter]
        public EventCallback<IDirectoryEntryAdapter> SelectedEntryChanged { get; set; }
        /// <summary>
        /// Text to show at the end of the TreeView item
        /// </summary>
        [Parameter]
        public Func<IDirectoryEntryAdapter?, string>? EndText { get; set; }

        protected Color GetItemColor(IDirectoryEntryAdapter? item)
        {
            if (item is IAccountDirectoryAdapter account)
            {
                if (account.Disabled)
                {
                    return Color.Error;
                }

                if (account.LockedOut)
                {
                    return Color.Warning;
                }

                if (account.Created > DateTime.Now.AddDays(-14))
                {
                    return Color.Success;
                }
            }
            return Color.Default;
        }

        protected bool GetExpanded(ITreeItemData<IDirectoryEntryAdapter> item)
        {
            return item.Expanded;
        }
        protected virtual IReadOnlyCollection<TreeItemData<IDirectoryEntryAdapter>> GetItems(ITreeItemData<IDirectoryEntryAdapter>? parent)
        {
            try
            {
                if (parent?.Expanded == true || parent?.Value?.CachedChildren != null)
                {
                    var items = parent?.Children
                        .Where(c => c.Value.ObjectType == ActiveDirectoryObjectType.OU && ShouldShowOU(c.Value))
                        .Select(p => p.Value);

                    var treeBranch = items?.ToTreeItemData();
                    //OpenToSelected(treeBranch);
                    return treeBranch ?? [];
                }
                return [];
            }
            catch (Exception)
            {
                return [];

            }

        }

        protected void InitializeTreeView()
        {
            LoadingData = true;
            _ = StateHasChangedAsync();
            if (RootOU is null || RootOU.Count < 1)
            {
                TopLevel = new ADOrganizationalUnit();
                TopLevel.Parse(directory: Directory, directoryEntry: Directory.GetDirectoryEntry());
                _ = TopLevel.SubOUs;
                var TopLevelList = new List<IDirectoryEntryAdapter>() { TopLevel };
                RootOU = TopLevelList.ToTreeItemData();
            }

            OpenToSelected(RootOU);



            LoadingData = false;
        }












        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            _ = Task.Run(() =>
            {
                InitializeTreeView();
            });
        }

        protected void OpenToSelected(IReadOnlyCollection<TreeItemData<IDirectoryEntryAdapter>>? rootOU)
        {
            if (rootOU == null || !rootOU.Any())
            {
                return;
            }

            var root = rootOU.First();
            root.Children = GetChildren(root);

            if (!StartRootExpanded)
            {
                return;
            }

            root.Expanded = true;

            if (SelectedEntry == null)
            {
                root.Selected = true;
                SelectedEntry = root.Value;
                return;
            }

            if (root.Value?.Equals(SelectedEntry) == true)
            {
                root.Selected = true;
                return;
            }
            ITreeItemData<IDirectoryEntryAdapter>? currentNode = root;
            while (currentNode != null)
            {
                currentNode.Children = GetChildren(currentNode);
                var nextNode = currentNode.Children.FirstOrDefault(IsAncestorOfSelected);

                if (nextNode != null)
                {
                    nextNode.Expanded = true;
                    currentNode = nextNode;
                }
                else
                {
                    SelectFinalNode(currentNode);
                    break;
                }
            }
        }
        private bool IsAncestorOfSelected(ITreeItemData<IDirectoryEntryAdapter> item)
        {
            return SelectedEntry?.DN?.Contains(item.Value.DN) == true && !SelectedEntry.DN.Equals(item.Value.DN);
        }

        private void SelectFinalNode(ITreeItemData<IDirectoryEntryAdapter> parent)
        {
            parent.Children?.ForEach(c =>
            {
                if (c.Value is IADOrganizationalUnit ou)
                {
                    c.Children = ou.SubOUs.ToTreeItemData();
                }
            });

            var matchingOU = parent.Children?.FirstOrDefault(c => SelectedEntry.DN.Equals(c.Value.DN));
            if (matchingOU != null)
            {
                matchingOU.Selected = true;
            }
        }
        /// <summary>
        /// Defines a function to determine whether an Active Directory object should be
        /// displayed in the tree view or not
        /// </summary>
        [Parameter]
        public Func<IDirectoryEntryAdapter, bool>? AdditionalVisibilityFilters { get; set; }


        protected bool ShouldShowOU(IDirectoryEntryAdapter entry)
        {
            if (entry is IADOrganizationalUnit ou)
            {
                if (ou.CanRead)
                {
                    return true;
                }

                if (AdditionalVisibilityFilters != null)
                {
                    if (AdditionalVisibilityFilters(entry))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        protected bool CanExpand(ITreeItemData<IDirectoryEntryAdapter> item)
        {
            return (item.Value is IADOrganizationalUnit);
        }
        protected IReadOnlyCollection<ITreeItemData<IDirectoryEntryAdapter>> GetChildren(ITreeItemData<IDirectoryEntryAdapter> context)
        {
            if (context.Children?.Count > 0)
            {
                return context.Children;
            }
            if (context.Expanded && context.Value is IADOrganizationalUnit ou)
            {
                return GetOUChildren(ou);
            }
            return [];
        }

        protected IReadOnlyCollection<ITreeItemData<IDirectoryEntryAdapter>> GetOUChildren(IDirectoryEntryAdapter ou)
        {
            if (ou is IADOrganizationalUnit context)
            {
                return context.TreeViewSubOUs.Where(o => ShouldShowOU(o)).ToTreeItemData();
            }
            return [];

        }
        protected async Task<IReadOnlyCollection<ITreeItemData<IDirectoryEntryAdapter>>> GetOUChildrenAsync(IDirectoryEntryAdapter parentNode)
        {
            return await Task.Run(() =>
            {
                return GetOUChildren(parentNode);


            });
        }
    }
}