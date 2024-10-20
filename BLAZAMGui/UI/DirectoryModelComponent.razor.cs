using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Models;
using BLAZAM.Gui.UI.Settings;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Gui.UI
{
    public partial class DirectoryModelComponent : ValidatedForm
    {
        protected bool EditMode = false;

        protected string _searchTerm;
        private IADUser _user;
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
        public IADComputer Computer
        {
            get; set;
        }
        [Parameter]
        public IADUser User
        {
            get => _user; set
            {
                if (_user == value) return;
                _user = value;
                if (_user != null)
                {
                    //if (!_user.NewEntry)
                    // AuditLogger.User.Searched(_user);
                    RefreshGroupGroupsAsync();
                }

            }
        }
        protected List<IADGroup> memberOfGroups = new();

        [Parameter]
        public IADOrganizationalUnit OU { get; set; }

        //bool _savingChanges = false;
        ///// <summary>
        ///// Indicates whether changes to the model are being saved right now.
        ///// </summary>
        //[Parameter]
        //public bool SavingChanges
        //{
        //    get => _savingChanges;
        //    set
        //    {
        //        if (value == _savingChanges) return;
        //        _savingChanges = value;
        //        SavingChangesChanged.InvokeAsync(value);

        //    }
        //}
        //[Parameter]
        //public EventCallback<bool> SavingChangesChanged { get; set; }




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

            if (User != null)
                User.OnModelChanged += (() => { InvokeAsync(StateHasChanged); });

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
            await InvokeAsync(StateHasChanged);
            if (User != null)
                memberOfGroups = User.MemberOf;

            LoadingData = false;

            await InvokeAsync(StateHasChanged);


        }
        protected async Task RefreshGroupGroupsAsync()
        {

            await RefreshGroupGroups();


        }
        protected async Task RefreshGroupGroups()
        {

            LoadingData = true;
            await InvokeAsync(StateHasChanged);

            if (Group != null)
                memberOfGroups = Group.MemberOf;
            LoadingData = false;
            await InvokeAsync(StateHasChanged);


        }
        protected async Task RefreshComputerGroups()
        {

            LoadingData = true;
            await InvokeAsync(StateHasChanged);

            if (Computer != null)
                memberOfGroups = Computer.MemberOf;


            LoadingData = false;

            await InvokeAsync(StateHasChanged);


        }



    }
}