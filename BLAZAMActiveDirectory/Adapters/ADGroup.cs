using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Searchers;
using BLAZAM.Common.Data;
using BLAZAM.Jobs;

namespace BLAZAM.ActiveDirectory.Adapters
{


    public class ADGroup : GroupableDirectoryAdapter, IADGroup
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
        public override List<AuditChangeLog> Changes
        {
            get
            {
                List<AuditChangeLog> changes = base.Changes;
                if (MembersToAdd.Count > 0 || MembersToRemove.Count > 0)
                {
                    var members = MembersAsStrings;
                    members.AddRange(MembersToAdd.Select(gm => gm.Member.DN));
                    MembersToRemove.ForEach(gm =>
                    {
                        members.Remove(gm.Member.DN);
                    });
                    changes.Add(new AuditChangeLog()
                    {
                        Field = "member",
                        OldValue = MembersAsStrings,
                        NewValue = members
                    });
                }

                return changes;

            }
        }
        public override IJob CommitChanges(IJob? dcr = null)
        {
            //dcr ??= new DirectoryChangeResult();
            var newMembers = new List<string>(MembersAsStrings);
            if (MembersToAdd.Count > 0)
            {
                CommitSteps.Add(new Jobs.JobStep("Add group members", () => {
                    MembersToAdd.ForEach(g =>
                    {
                        g.Group.Invoke("Add", new object[] { g.Member.ADSPath });
                        //dcr.AssignedMembers.Add(g.Group);

                    });
                    return true;
                }));

               
            }
            if (MembersToRemove.Count > 0)
            {
                CommitSteps.Add(new Jobs.JobStep("Remove group members", () => {
                    MembersToRemove.ForEach(g =>
                    {
                        g.Group.Invoke("Remove", new object[] { g.Member.ADSPath });
                        //dcr.UnassignedMembers.Add(g.Group);
                    });
                    return true;
                }));
             
            }

            dcr = base.CommitChanges(dcr);

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
                if (_userMembersCache == null)
                {
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
        public List<string> MembersAsStrings
        {
            get
            {
                var temp = GetStringListProperty("member");
                return temp;
            }
        }
        public IEnumerable<IGroupableDirectoryAdapter> NestedMembers
        {
            get
            {
                ADSearch search = new ADSearch();
                search.Fields.NestedMemberOf = this;
                var result = search.Search<GroupableDirectoryAdapter, IGroupableDirectoryAdapter>();
                return result;
            }
        }
        public List<IGroupableDirectoryAdapter> Members
        {
            get
            {
                var temp = MembersAsStrings;
                ADSearch search = new ADSearch();

                List<IGroupableDirectoryAdapter> members = new List<IGroupableDirectoryAdapter>();
                temp.ForEach(t =>
                {
                    search.Results.Clear();
                    search.Fields.DN = t;
                    var member = search.Search<GroupableDirectoryAdapter, IGroupableDirectoryAdapter>()?.FirstOrDefault();
                    if (member != null)
                    {
                        members.Add(member);
                    }

                });
                var tempRemoval = new List<IGroupableDirectoryAdapter>(members);
                tempRemoval.ForEach(m =>
                {
                    if (MembersToRemove.Select(gm => gm.Member).Contains(m))
                    {
                        members.Remove(m);
                    }
                });
                MembersToAdd.ForEach(m =>
                {
                    if (!members.Contains(m.Member))
                        members.Add(m.Member);
                });
                return members;
            }
        }

        public void UnassignMember(IGroupableDirectoryAdapter member)
        {

            MembersToRemove.Add(new GroupMembership(this, member));
            HasUnsavedChanges = true;
            return;



        }

        public void AssignMember(IGroupableDirectoryAdapter member)
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
