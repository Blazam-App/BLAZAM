
using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Models.Database.Permissions;

namespace BLAZAM.Common.Data.ActiveDirectory.Models
{


    public class ADGroup : GroupableDirectoryModel, IADGroup
    {
        public override string SearchUri
        {
            get
            {
                return "/groups/search/" + CanonicalName;
            }
        }
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
