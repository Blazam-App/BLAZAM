using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Models.Database.Permissions;
using System.Data;
using System.Globalization;

namespace BLAZAM.Common.Data.ActiveDirectory.Models
{
    public class GroupableDirectoryAdapter : DirectoryEntryAdapter, IGroupableDirectoryAdapter
    {
        const int ADS_UF_ACCOUNTDISABLE = 0x0002;
        const int ADS_UF_PASSWD_NOTREQD = 0x0020;
        const int ADS_UF_PASSWD_CANT_CHANGE = 0x0040;
        const int ADS_UF_NORMAL_ACCOUNT = 0x0200;
        const int ADS_UF_DONT_EXPIRE_PASSWD = 0x10000;
        const Int32 ACCOUNT_ENABLE_MASK = 0xFFFFFFD;

        DateTime ADS_NULL_TIME = DateTime.ParseExact("12/31/1600 7:00:00 PM", "MM/dd/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);

        protected List<GroupMembership> ToAssignTo = new List<GroupMembership>();
        protected List<GroupMembership> ToUnassignFrom = new List<GroupMembership>();
       

        public virtual bool CanAssign
        {
            get
            {
                return HasPermission(p => p.Where(pm =>
               pm.AccessLevels.Any(al => al.ActionMap.Any(om =>
               om.ObjectType == ObjectType &&
               om.ObjectAction.Name == ActionAccessFlags.Assign.Name &&
               om.AllowOrDeny
               ))),
               p => p.Where(pm =>
               pm.AccessLevels.Any(al => al.ActionMap.Any(om =>
               om.ObjectType == ObjectType &&
               om.ObjectAction.Name == ActionAccessFlags.Assign.Name &&
               !om.AllowOrDeny
               )))
               );
            }
        }

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
                
                temp.AddRange(ToAssignTo.Select(gm=>gm.Group).ToList());
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

        public virtual bool CanEnable { get => HasActionPermission(ActionAccessFlags.Enable); }

        public virtual bool CanDisable { get => HasActionPermission(ActionAccessFlags.Disable); }
       
        public virtual bool CanUnlock { get => HasActionPermission(ActionAccessFlags.Unlock); }

        public virtual DateTime? LockoutTime
        {
            get
            {
                var com = GetProperty<object>("lockoutTime");
                return com.AdsValueToDateTime();
            }
        }

        public virtual bool LockedOut
        {
            get
            {

                var date = LockoutTime;
                return date != ADS_NULL_TIME && date != DateTime.MinValue;
            }
            set
            {
                if (value)
                {
                    SetProperty("lockoutTime", DateTime.UtcNow);
                }
                else
                {

                    SetProperty("lockoutTime", 0);
                }
            }
        }

        public virtual bool Disabled
        {
            get
            {

                try
                {
                    return (UAC & ADS_UF_ACCOUNTDISABLE) == ADS_UF_ACCOUNTDISABLE;
                }
                catch
                {
                    // handle NullReferenceException
                }
                return true;
            }
            set
            {
                if (value && !Disabled)
                {
                    UAC = (UAC | ADS_UF_ACCOUNTDISABLE);
                }
                else if (!value && Disabled)
                {
                    
                    UAC = (UAC & ACCOUNT_ENABLE_MASK);

                }
            }
        }
      
        protected Int32 UAC
        {
            get
            {
                return Convert.ToInt32(GetProperty<object>("userAccountControl"));
            }
            set
            {
                SetProperty("userAccountControl", value);
            }
        }

        /// <summary>
        /// Overridden CanRead to check that the application user has permission
        /// to disabled objects of this objects type on top of normal read
        /// permission checks
        /// </summary>
        public override bool CanRead
        {
            get
            {
                if (Disabled && CanSearchDisabled)
                {

                    return HasPermission(p => p.Where(pm =>
                pm.AccessLevels.Any(al =>
                al.ObjectMap.Any(om =>
                om.ObjectType == ObjectType &&
                om.ObjectAccessLevel.Level > ObjectAccessLevels.Deny.Level &&
                om.AllowDisabled
                ))),
                p => p.Where(pm =>
                pm.AccessLevels.Any(al =>
                al.ObjectMap.Any(om =>
                om.ObjectType == ObjectType &&
                (om.ObjectAccessLevel.Level == ObjectAccessLevels.Deny.Level ||
                !om.AllowDisabled)
                )))
                );
                }
                else
                {
                    if (!Disabled)
                    {
                        return base.CanRead;
                    }
                }
                return false;
            }
        }
      
        /// <summary>
        /// Checks that the user has some kind of read access for disabled objects of
        /// this type in any place on the OU tree.
        /// </summary>
        public bool CanSearchDisabled
        {
            get
            {
                if (UserStateService.CurrentUserState?.IsSuperAdmin==true) return true;
                return UserStateService.CurrentUserState.DirectoryUser.PermissionMappings.Any(pm => pm.AccessLevels.Any(al => al.ObjectMap.Any(om => om.ObjectType == ObjectType && om.AllowDisabled)));

            }
        }

        public bool Enabled { get => !Disabled; set => Disabled=!value; }
        public virtual DateTime? ExpireTime
        {
            get
            {
                var com = GetProperty<object>("accountExpires")?.AdsValueToDateTime();
                if (com == ADS_NULL_TIME || com == DateTime.MinValue) com = null;
                return com;
            }
            set
            {
                SetProperty("accountExpires", value.DateTimeToAdsValue());
            }
        }

        public override List<AuditChangeLog> Changes
        {
            get
            {
                List<AuditChangeLog> changes = base.Changes;
                if(ToAssignTo.Count>0 || ToUnassignFrom.Count > 0)
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
            DirectoryChangeResult  dcr = new DirectoryChangeResult();
            ToAssignTo.ForEach(g => {
                g.Group.Invoke("Add", new object[] { g.Member.ADSPath });
                dcr.AssignedGroups.Add(g.Group);

            });
            ToUnassignFrom.ForEach(g => {
                g.Group.Invoke("Remove", new object[] { g.Member.ADSPath });
                dcr.UnassignedGroups.Add(g.Group);
            });
            dcr =base.CommitChanges();
            
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
