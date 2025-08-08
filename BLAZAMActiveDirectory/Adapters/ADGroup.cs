using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Searchers;
using BLAZAM.Common.Data;
using BLAZAM.Database.Models;
using BLAZAM.Jobs;

namespace BLAZAM.ActiveDirectory.Adapters
{

    public enum GroupScope
    {
        Universal,
        Global,
        DomainLocal
    }
    public class ADGroup : GroupableDirectoryAdapter, IADGroup
    {
        protected const int ADS_GROUP_TYPE_GLOBAL_GROUP = 0x2;
        protected const int ADS_GROUP_TYPE_DOMAIN_LOCAL_GROUP = 0x4;
        protected const int ADS_GROUP_TYPE_UNIVERSAL_GROUP = 0x8;
        protected const int ADS_GROUP_TYPE_SECURITY_ENABLED = unchecked((int)0x80000000);

        public GroupScope GroupScope
        {
            get
            {
                if (IsDomainLocalGroup) return GroupScope.DomainLocal;
                if (IsGlobalGroup) return GroupScope.Global;
                return GroupScope.Universal;
            }
            set
            {
                switch (value)
                {
                    case GroupScope.Universal:

                        GroupType = GroupType | ADS_GROUP_TYPE_UNIVERSAL_GROUP;
                        GroupType = GroupType & ~ADS_GROUP_TYPE_GLOBAL_GROUP;
                        GroupType = GroupType & ~ADS_GROUP_TYPE_DOMAIN_LOCAL_GROUP;

                        break;
                    case GroupScope.Global:
                        GroupType = GroupType | ADS_GROUP_TYPE_GLOBAL_GROUP;
                        GroupType = GroupType & ~ADS_GROUP_TYPE_UNIVERSAL_GROUP;
                        GroupType = GroupType & ~ADS_GROUP_TYPE_DOMAIN_LOCAL_GROUP;
                        break;
                    case GroupScope.DomainLocal:
                        GroupType = GroupType | ADS_GROUP_TYPE_DOMAIN_LOCAL_GROUP;
                        GroupType = GroupType & ~ADS_GROUP_TYPE_GLOBAL_GROUP;
                        GroupType = GroupType & ~ADS_GROUP_TYPE_UNIVERSAL_GROUP;
                        break;

                }
            }
        }

        public virtual string? SAMAccountName
        {

            get
            {
                return GetStringAttribute(ActiveDirectoryFields.SAMAccountName.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.SAMAccountName.FieldName, value);
            }


        }

        public bool IsSecurityGroup
        {
            get
            {
                return (GroupType & ADS_GROUP_TYPE_SECURITY_ENABLED) != 0;
            }
            set
            {
                if (value)
                {
                    GroupType = GroupType | ADS_GROUP_TYPE_SECURITY_ENABLED;
                }
                else
                {
                    GroupType = GroupType & ~ADS_GROUP_TYPE_SECURITY_ENABLED;

                }
            }
        }
        public bool IsGlobalGroup
        {
            get
            {
                return (GroupType & ADS_GROUP_TYPE_GLOBAL_GROUP) != 0;
            }

        }
        public bool IsDomainLocalGroup
        {
            get
            {
                return (GroupType & ADS_GROUP_TYPE_DOMAIN_LOCAL_GROUP) != 0;
            }

        }
        public bool IsUniversalGroup
        {
            get
            {
                return (GroupType & ADS_GROUP_TYPE_UNIVERSAL_GROUP) != 0;

            }

        }


        public List<GroupMembership> MembersToRemove { get; private set; } = new List<GroupMembership>();
        public List<GroupMembership> MembersToAdd { get; private set; } = new List<GroupMembership>();
        public override string? DisplayName { get => base.CanonicalName; set => base.CanonicalName = value; }
        public string? GroupName
        {

            get
            {
                return GetStringAttribute("name");
            }
            set
            {
                SetAttribute("name", value);
            }
        }

        public override bool Rename(string newName)
        {
            SAMAccountName = newName;
            CommitChanges();
            return base.Rename(newName);
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


        public override IJob CommitChanges(IJob? commitJob = null)
        {
            if (MembersToAdd.Count > 0)
            {
                PostCommitSteps.Add(new JobStep("Add group members", (JobStep? step) =>
                {
                    MembersToAdd.ForEach(g =>
                    {
                        g.Group.Invoke("Add", new object[] { g.Member.ADSPath });

                    });
                    return true;
                }));


            }
            if (MembersToRemove.Count > 0)
            {
                PostCommitSteps.Add(new JobStep("Remove group members", (JobStep? step) =>
                {
                    MembersToRemove.ForEach(g =>
                    {
                        g.Group.Invoke("Remove", new object[] { g.Member.ADSPath });
                    });
                    return true;
                }));

            }

            commitJob = base.CommitChanges(commitJob);

            return commitJob;
        }

        public override void DiscardChanges()
        {
            MembersToRemove = new();
            MembersToAdd = new();
            CachedChildren = new List<IDirectoryEntryAdapter>();
            _groupMembersCache = new List<IADGroup>();
            _userMembersCache = new();
            base.DiscardChanges();
            OnModelChanged?.Invoke();

        }
        public bool HasMembers => UserMembers.Count > 0 || GroupMembers.Count > 0;

        private List<IADUser> _userMembersCache;

        /// <summary>
        /// The members of this group, that are users themselves
        /// </summary>
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

        private List<IADGroup> _groupMembersCache;
        /// <summary>
        /// The members of this group, that are groups themselves
        /// </summary>
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
        public List<string>? MembersAsStrings
        {
            get
            {
                var temp = GetStringListAttribute("member");
                return temp;
            }
        }
        protected int GroupType
        {
            get
            {
                var uacRaw = Convert.ToInt32(GetAttribute<object>("groupType"));

                return uacRaw;
            }
            set
            {
                SetAttribute("groupType", value);
            }
        }
        /// <summary>
        /// Gathers group and sub-group members in realtime
        /// </summary>
        public IEnumerable<IGroupableDirectoryAdapter> NestedMembers
        {
            get
            {
                ADSearch search = new(Directory);
                search.Fields.NestedMemberOf = this;
                var result = search.Search<GroupableDirectoryAdapter, IGroupableDirectoryAdapter>();
                return result;
            }
        }

        /// <summary>
        /// Gathers current group members in realtime
        /// </summary>
        public List<IGroupableDirectoryAdapter> Members
        {
            get
            {
                var temp = MembersAsStrings;
                ADSearch search = new(Directory);

                List<IGroupableDirectoryAdapter> members = new();
                temp?.ForEach(t =>
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
        /// <summary>
        /// Removes a member from this group
        /// </summary>
        /// <param name="member">The user or group to remove</param>
        public void UnassignMember(IGroupableDirectoryAdapter member)
        {

            MembersToRemove.Add(new GroupMembership(this, member));
            HasUnsavedChanges = true;
        }
        /// <summary>
        /// Assigns a member to this group
        /// </summary>
        /// <param name="member"></param>
        public void AssignMember(IGroupableDirectoryAdapter member)
        {

            MembersToAdd.Add(new GroupMembership(this, member));
            HasUnsavedChanges = true;
        }
        public int CompareTo(object? obj)
        {
            if (obj is ADGroup g)
                return CanonicalName.CompareTo(g.CanonicalName);
            return 0;
        }


    }
}
