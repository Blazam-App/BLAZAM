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
        public IReadOnlyCollection<TreeItemData<IDirectoryEntryAdapter>>? RootOU { get; set; } = new List<TreeItemData<IDirectoryEntryAdapter>>();
        protected IReadOnlyCollection<TreeItemData<IDirectoryEntryAdapter>>? GuiOU { get; set; } = new List<TreeItemData<IDirectoryEntryAdapter>>();
        [Parameter]
        public IADOrganizationalUnit? StartingSelectedOU
        {
            get => _startingSelectedNode; set
            {
                if (value == _startingSelectedNode) return;
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
                if (value == _selectedEntry) return;
                if (value != null)
                {
                    var cache = _selectedEntry;

                    _selectedEntry = value;
                    if (cache == null && RootOU?.Count > 0 && value == RootOU.First()) return;


                    InvokeAsync(() => { SelectedEntryChanged.InvokeAsync(value); });

                }
            }

        }
        protected Color GetIconColor(TreeItemData<IDirectoryEntryAdapter> context)
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
                if (account.Disabled) return Color.Error;
                if (account.LockedOut) return Color.Warning;
                if (account.Created > DateTime.Now.AddDays(-14)) return Color.Success;
            }
            return Color.Default;
        }

        public async Task RefreshViewAcync()
        {
            await InvokeAsync(StateHasChanged);
        }

        protected virtual IReadOnlyCollection<TreeItemData<IDirectoryEntryAdapter>> GetItems(IDirectoryEntryAdapter? parent)
        {
            try
            {

                var items = parent?.Children
                    .Where(c => c.ObjectType == ActiveDirectoryObjectType.OU && ShouldShowOU(c));


                var treeBranchh = items?.ToTreeItemData();
                return treeBranchh?? new List<TreeItemData<IDirectoryEntryAdapter>>();

            }
            catch (Exception)
            {
                return new List<TreeItemData<IDirectoryEntryAdapter>>();

            }

        }

        protected void InitializeTreeView()
        {
            LoadingData = true;
            _ = InvokeAsync(StateHasChanged);
            if (RootOU is null || RootOU.Count < 1)
            {
                TopLevel = new ADOrganizationalUnit();
                TopLevel.Parse(directory: Directory, directoryEntry: Directory.GetDirectoryEntry());
                _ = TopLevel.SubOUs;
                var TopLevelList = new List<IDirectoryEntryAdapter>() { TopLevel };
                RootOU = TopLevelList.ToTreeItemData();
            }

            OpenToSelected();

            GuiOU = new List<TreeItemData<IDirectoryEntryAdapter>>(RootOU);

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

        protected void OpenToSelected()
        {
            RootOU.First().Children = GetChildren(RootOU.First());
            if (StartRootExpanded && RootOU != null && RootOU.Count > 0)
            {
                RootOU.First().Expanded = true;

                if (SelectedEntry != null)
                {
                    var firstThing = RootOU.First();
                    if (firstThing is TreeItemData<IDirectoryEntryAdapter> openThis)
                    {
                        if (!SelectedEntry.Equals(RootOU.First().Value))
                        {
                            openThis.Expanded = true;
                        }

                        while (openThis != null)
                        {

                            openThis.Children = GetChildren(openThis);
                            var child = openThis.Children.FirstOrDefault(
                                c => SelectedEntry.DN?.Contains(c.Value.DN) == true
                                                            && !SelectedEntry.DN.Equals(c.Value.DN)
                                                            );
                            if (child != null)
                            {
                                if (!SelectedEntry.Equals(RootOU.First().Value))
                                {
                                    child.Expanded = true;
                                }
                                openThis = child;
                            }
                            else
                            {
                                openThis.Children.ForEach(c =>
                                {
                                    if (c.Value is IADOrganizationalUnit ou)
                                    {
                                        c.Children = ou.SubOUs.ToTreeItemData();
                                    }
                                });

                                var matchingOU = openThis.Children.FirstOrDefault(c => SelectedEntry.DN.Equals(c.Value.DN));
                                if (matchingOU != null)
                                    matchingOU.Selected = true;
                                break;
                            }


                        }
                    }
                }
                else
                {
                    RootOU.First().Selected = true;
                    SelectedEntry = RootOU.First().Value;
                }
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
                    return true;
                if (AdditionalVisibilityFilters != null)
                {
                    if (AdditionalVisibilityFilters(entry)) return true;
                }
            }
            return false;
        }


        protected List<TreeItemData<IDirectoryEntryAdapter>> GetChildren(TreeItemData<IDirectoryEntryAdapter> context)
        {
            if (context.Children?.Count > 0)
            {
                return context.Children;
            }
            if (context.Value is IADOrganizationalUnit ou)
            {
                return GetOUChildren(ou);
            }
            return new List<TreeItemData<IDirectoryEntryAdapter>>();
        }
        protected bool CanExpand(TreeItemData<IDirectoryEntryAdapter> item)
        {
            return item.Value is IADOrganizationalUnit;
        }
        protected List<TreeItemData<IDirectoryEntryAdapter>> GetOUChildren(IDirectoryEntryAdapter ou)
        {
            if (ou is IADOrganizationalUnit context)
            {
                return context.TreeViewSubOUs.Where(o => ShouldShowOU(o)).ToTreeItemData();
            }
            return new List<TreeItemData<IDirectoryEntryAdapter>>();

        }
        protected async Task<IReadOnlyCollection<TreeItemData<IDirectoryEntryAdapter>?>> GetOUChildrenAsync(IDirectoryEntryAdapter parentNode)
        {
            return await Task.Run(() =>
            {
                return GetOUChildren(parentNode);


            });
        }
    }
}