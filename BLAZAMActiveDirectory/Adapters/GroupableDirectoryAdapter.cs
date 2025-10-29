using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using System.Data;
using System.Text.Json.Serialization;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class GroupableDirectoryAdapter : DirectoryEntryAdapter, IGroupableDirectoryAdapter
    {
        [JsonIgnore]
        public List<GroupMembership> ToAssignTo { get; protected set; } = [];

        [JsonIgnore]
        public List<GroupMembership> ToUnassignFrom { get; protected set; } = [];

        public virtual bool CanAssign => HasActionPermission(ObjectActions.Assign);

        public virtual bool CanUnassign => HasActionPermission(ObjectActions.UnAssign);

        private bool? _isAMember;

        public virtual bool IsAGroupMember
        {
            get
            {
                if (_isAMember == null)
                {
                    var temp = MemberOf;
                    _isAMember = temp != null && temp.Count > 0;
                }
                return (bool)_isAMember;
            }
        }

        public virtual bool IsAMemberOf(IADGroup? group)
        {
            if (group is null)
            {
                return false;
            }
            return Directory.Groups.IsAMemberOf(group, this, true);

        }


        private List<IADGroup> _memberOf;
        /// <summary>
        /// Returns the groups this entry is a member of.
        /// </summary>
        /// <remarks>
        /// It also adds the pending groups to be added, and the groups to be removed from.
        /// </remarks>
        [JsonIgnore]
        public virtual List<IADGroup> MemberOf
        {
            get
            {
                if (_memberOf == null)
                {
                    _memberOf = Directory.Groups.FindGroupsByDN(GetStringListAttribute(ActiveDirectoryFields.MemberOf.FieldName)).OrderBy(g => g.CanonicalName).ToList();

                }
                var temp = new List<IADGroup>(_memberOf);

                temp.AddRange(ToAssignTo.Select(gm => gm.Group).ToList());
                ToUnassignFrom.ForEach(g => temp.Remove(g.Group));
                return temp;
            }

        }

        public virtual string? DisplayName
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.DisplayName.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.DisplayName.FieldName, value);
            }
        }

        public virtual string? Email
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.Mail.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.Mail.FieldName, value);
            }
        }

        public virtual string? Description
        {
            get
            {
                var raw = GetStringListAttribute(ActiveDirectoryFields.Description.FieldName);
                return raw?.FirstOrDefault();
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.Description.FieldName, value);
            }
        }




        [JsonIgnore]
        public override List<AuditChangeLog> Changes
        {
            get
            {
                List<AuditChangeLog> changes = base.Changes;
                if (ToAssignTo.Count > 0 || ToUnassignFrom.Count > 0)
                {
                    _ = MemberOf;
                    changes.Add(new AuditChangeLog()
                    {
                        Field = ActiveDirectoryFields.MemberOf.FieldName,
                        OldValue = _memberOf.Select(m => m.CanonicalName).ToList(),
                        NewValue = MemberOf.Select(m => m.CanonicalName).ToList()
                    });
                }

                return changes;

            }
        }

        public override IJob CommitChanges(IJob? commitJob = null)
        {
            if (ToAssignTo.Count > 0)
            {
                PostCommitSteps.Add(new Jobs.JobStep("Assign to groups", (step) =>
                {
                    ToAssignTo.ForEach(g =>
                    {
                        g.Group.Invoke("Add", new object[] { g.Member.DN });

                    });
                    return true;
                }));
            }
            if (ToUnassignFrom.Count > 0)
            {
                CommitSteps.Add(new JobStep("Unassign from groups", (step) =>
                {
                    ToUnassignFrom.ForEach(g =>
                    {
                        g.Group.Invoke("Remove", new object[] { g.Member.DN });
                    });
                    return true;
                }));

            }
            commitJob = base.CommitChanges(commitJob);

            return commitJob;
        }

        public override void DiscardChanges()
        {
            base.DiscardChanges();
            ToAssignTo = [];
            ToUnassignFrom = [];
        }



        public void AssignTo(IADGroup group)
        {

            ToAssignTo.Add(new GroupMembership(group, this));
            HasUnsavedChanges = true;
        }

        public void UnassignFrom(IADGroup group)
        {

            ToUnassignFrom.Add(new GroupMembership(group, this));
            HasUnsavedChanges = true;
        }

    }
}
