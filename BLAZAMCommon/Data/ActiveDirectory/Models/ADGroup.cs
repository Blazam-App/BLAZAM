
using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Models.Database.Permissions;
using Microsoft.VisualStudio.Services.Common;

namespace BLAZAM.Common.Data.ActiveDirectory.Models
{


    public class ADGroup : GroupableDirectoryModel, IADGroup
    {
        protected List<GroupMembership> MembersToRemove = new List<GroupMembership>();
        protected List<GroupMembership> MembersToAdd = new List<GroupMembership>();
        public override string? DisplayName { get => base.CanonicalName; set => base.CanonicalName = value; }
        public string GroupName
        {

            get
            {
                return GetStringProperty("name");
            }
            set
            {
                SetProperty("name", value);
            }
        }

        public override DirectoryChangeResult CommitChanges()
        {
            DirectoryChangeResult dcr = new DirectoryChangeResult();
            var newMembers = new List<string>(Members);
            if (MembersToAdd.Count > 0)
            {

              
                MembersToAdd.ForEach(g =>
                {
                    g.Group.Invoke("Add", new object[] { g.Member.ADSPath });
                    dcr.AssignedMembers.Add(g.Group);
                });
            }
            if (MembersToRemove.Count > 0)
            {
       
                MembersToRemove.ForEach(g =>
                {
                    g.Group.Invoke("Remove", new object[] { g.Member.ADSPath });
                    dcr.UnassignedMembers.Add(g.Group);
                });
            }
          
            dcr = base.CommitChanges();

            return dcr;
        }

        public override void DiscardChanges()
        {
            base.DiscardChanges();

            MembersToRemove = new();
            MembersToAdd = new();
        }
        public bool HasMembers => UserMembers.Count > 0 || GroupMembers.Count > 0;
        List<IADUser> _userMembersCache;
        public List<IADUser> UserMembers
        {
            get
            {
                if (_userMembersCache == null) {
                    _userMembersCache = Directory.Groups.GetDirectUserMembers(this);
                }
                var temp = new List<IADUser>(_userMembersCache);
                temp.AddRange(MembersToAdd.Where(m => m.Member is IADUser).Select(m => m.Member).Cast<IADUser>());

                MembersToRemove.ForEach(u =>
                {
                    if (u.Member is IADUser user)
                    {
                        temp.Remove(user);
                    }
                });
                temp = temp.OrderBy(u => u.CanonicalName).ToList();
                return temp;
            }
        }
        List<IADGroup> _groupMembersCache;
        public List<IADGroup> GroupMembers
        {
            get
            {
                if (_groupMembersCache == null)
                {
                    _groupMembersCache = Directory.Groups.GetGroupMembers(this);
                }
                var temp = new List<IADGroup>(_groupMembersCache);
                temp.AddRange(MembersToAdd.Where(m => m.Member is IADGroup).Select(m => m.Member).Cast<IADGroup>());

                MembersToRemove.ForEach(u =>
                {
                    if (u.Member is IADGroup group)
                    {
                        temp.Remove(group);
                    }
                });

                temp = temp.OrderBy(u => u.CanonicalName).ToList();
                return temp;
            }
        }
        
        public List<string> Members {
            get{
                var temp = GetStringListProperty("member");
                return temp;
            }
}
        
        public void UnassignMember(IGroupableDirectoryModel member)
        {

            MembersToRemove.Add(new GroupMembership(this, member));
            HasUnsavedChanges = true;
            return;



        }
        public void AssignMember(IGroupableDirectoryModel member)
        {

            MembersToAdd.Add(new GroupMembership(this, member));
            HasUnsavedChanges = true;
            return;



        }
        public int CompareTo(object? obj)
        {
            if (obj != null && obj is ADGroup g)
                return CanonicalName.CompareTo(g.CanonicalName);
            return 0;
        }

    
    }
}
