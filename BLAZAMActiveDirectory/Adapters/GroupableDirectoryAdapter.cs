using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Helpers;
using System.Data;
using System.Globalization;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class GroupableDirectoryAdapter : DirectoryEntryAdapter, IGroupableDirectoryAdapter
    {
      
        protected List<GroupMembership> ToAssignTo = new List<GroupMembership>();
        protected List<GroupMembership> ToUnassignFrom = new List<GroupMembership>();

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

        public virtual bool IsAMemberOf(IADGroup group)
        {
            return Directory.Groups.IsAMemberOf(group, this, true);

        }


        private List<IADGroup> _memberOf;
        /// <summary>
        /// Returns the groups this entry is a member of.
        /// </summary>
        /// <remarks>
        /// It also adds the pending groups to be added, and the groups to be removed from.
        /// </remarks>
        public virtual List<IADGroup> MemberOf
        {
            get
            {
                if (_memberOf == null)
                {
                    _memberOf = Directory.Groups.FindGroupsByDN(GetStringListProperty("memberOf")).OrderBy(g => g.CanonicalName).ToList();

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
                return GetStringProperty("displayName");
            }
            set
            {
                SetProperty("displayName", value);
            }
        }

        public virtual string? Email
        {
            get
            {
                return GetStringProperty("mail");
            }
            set
            {
                SetProperty("mail", value);
            }
        }

        public virtual string? Description
        {
            get
            {
                return GetStringProperty("description");
            }
            set
            {
                SetProperty("description", value);
            }
        }

      

     
    
        public override List<AuditChangeLog> Changes
        {
            get
            {
                List<AuditChangeLog> changes = base.Changes;
                if (ToAssignTo.Count > 0 || ToUnassignFrom.Count > 0)
                {
                    changes.Add(new AuditChangeLog()
                    {
                        Field = "memberOf",
                        OldValue = _memberOf.Select(m => m.CanonicalName).ToList(),
                        NewValue = MemberOf.Select(m => m.CanonicalName).ToList()
                    });
                }

                return changes;

            }
        }

        public override DirectoryChangeResult CommitChanges()
        {
            DirectoryChangeResult dcr = new DirectoryChangeResult();
            dcr = base.CommitChanges();

            ToAssignTo.ForEach(g =>
            {
                g.Group.Invoke("Add", new object[] { g.Member.ADSPath });
                dcr.AssignedGroups.Add(g.Group);

            });
            ToUnassignFrom.ForEach(g =>
            {
                g.Group.Invoke("Remove", new object[] { g.Member.ADSPath });
                dcr.UnassignedGroups.Add(g.Group);
            });

            return dcr;
        }

        public override void DiscardChanges()
        {
            base.DiscardChanges();
            ToAssignTo = new();
            ToUnassignFrom = new();
        }



        public void AssignTo(IADGroup group)
        {

            ToAssignTo.Add(new GroupMembership(group, this));
            HasUnsavedChanges = true;

            return;


        }

        public void UnassignFrom(IADGroup group)
        {

            ToUnassignFrom.Add(new GroupMembership(group, this));
            HasUnsavedChanges = true;
            return;



        }




    }
}
