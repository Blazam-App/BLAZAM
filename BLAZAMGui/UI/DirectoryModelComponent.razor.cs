using BLAZAM.Gui.UI.Settings;

namespace BLAZAM.Gui.UI
{
    public abstract class DirectoryModelComponent : ValidatedForm
    {
        protected bool EditMode = false;

        protected string _searchTerm;
        private IDirectoryEntryAdapter _entry;
        private IADGroup _group;

        [Parameter]
        public IADGroup Group
        {
            get => _group; set
            {
                if (_group == value) return;
                _group = value;

            }
        }

        private List<IADGroup> _groups;

        [Parameter]
        public List<IADGroup> Groups
        {
            get => _groups; set
            {
                if (_groups == value) return;
                _groups = value;
                if (_groups != null)
                {

                    RefreshGroupGroups();
                }
            }
        }
        protected IList<CustomActiveDirectoryField> CustomFields { get; set; } = new List<CustomActiveDirectoryField>();
        [Parameter]
        public IADUser User
        {
            get => Entry as IADUser; set => Entry = value;
        }
        [Parameter]
        public IADContact Contact
        {
            get => Entry as IADContact; set => Entry = value;
        }
        [Parameter]
        public IADComputer Computer
        {
            get => Entry as IADComputer; set => Entry = value;
        }
        [Parameter]
        public IGroupableDirectoryAdapter GroupableEntry
        {
            get => Entry as IGroupableDirectoryAdapter; set => Entry = value;
        }
        [Parameter]
        public virtual IDirectoryEntryAdapter Entry
        {
            get => _entry; set
            {
                if (_entry == value) return;
                _entry = value;
                EntryChanged.InvokeAsync(_entry);
                if (User != null)
                {
                    RefreshGroupGroupsAsync();
                }

            }
        }
        [Parameter]
        public EventCallback<IDirectoryEntryAdapter> EntryChanged { get; set; }
        protected List<IADGroup> memberOfGroups = new();

        [Parameter]
        public IADOrganizationalUnit OU { get; set; }


        /// <summary>
        /// Standard search page initializer that copies the url search term to the
        /// text search term if it is set.
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            if (_searchTerm == null || _searchTerm == "")
            {
                LoadingData = false;

            }
            if (Group != null)
                Group.OnModelChanged += (() => { InvokeAsync(StateHasChanged); });

            if (Entry != null)
                Entry.OnModelChanged += (() => { InvokeAsync(StateHasChanged); });

            if (Computer != null)
                Computer.OnModelChanged += (() => { InvokeAsync(StateHasChanged); });

            if (OU != null)
                OU.OnModelChanged += (() => { InvokeAsync(StateHasChanged); });

            if (Context != null)
                CustomFields = await Context.CustomActiveDirectoryFields.Where(cf => cf.DeletedAt == null).ToListAsync();
            await InvokeAsync(StateHasChanged);

        }



        protected async Task RefreshUserGroups()
        {

            LoadingData = true;
            await Task.Run(() =>
            {
                if (GroupableEntry != null)
                    memberOfGroups = GroupableEntry.MemberOf;

            });

            LoadingData = false;



        }
        protected async Task RefreshGroupGroupsAsync()
        {

            await RefreshGroupGroups();


        }
        protected async Task RefreshGroupGroups()
        {

            LoadingData = true;
            await Task.Run(() =>
            {
                if (Group != null)
                    memberOfGroups = Group.MemberOf;
            });

            LoadingData = false;


        }
        protected async Task RefreshComputerGroups()
        {

            LoadingData = true;
            await Task.Run(() =>
            {
                if (Computer != null)
                    memberOfGroups = Computer.MemberOf;
            });

            LoadingData = false;



        }



    }
}