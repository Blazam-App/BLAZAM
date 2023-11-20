using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Helpers;
using System.Data;
using System.Globalization;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class AccountDirectoryAdapter : GroupableDirectoryAdapter, IAccountDirectoryAdapter
    {
        const int ADS_UF_ACCOUNTDISABLE = 0x0002;
        const int ADS_UF_PASSWD_NOTREQD = 0x0020;
        const int ADS_UF_PASSWD_CANT_CHANGE = 0x0040;
        const int ADS_UF_NORMAL_ACCOUNT = 0x0200;
        const int ADS_UF_DONT_EXPIRE_PASSWD = 0x10000;
        const int ACCOUNT_ENABLE_MASK = 0xFFFFFFD;

        


        public virtual bool CanEnable { get => HasActionPermission(ObjectActions.Enable); }

        public virtual bool CanDisable { get => HasActionPermission(ObjectActions.Disable); }

        public virtual bool CanUnlock { get => HasActionPermission(ObjectActions.Unlock); }


        public bool CanSearchDisabled
        {
            get
            {
                PermissionMapping map = new();

                if (CurrentUser?.IsSuperAdmin == true) return true;
                return CurrentUser?.PermissionMappings.Any(pm => pm.AccessLevels.Any(al => al.ObjectMap.Any(om => om.ObjectType == ObjectType && om.AllowDisabled))) == true;

            }
        }


        public virtual DateTime? LockoutTime
        {
            get
            {
                return GetDateTimeProperty("lockoutTime");

            }
        }

        public DateTime? LastLogonTime
        {
            get
            {
                var coms = GetNonReplicatedProperty<object>("lastLogon");
                List<DateTime?> times = new List<DateTime?>();
                foreach (var c in coms)
                {
                    times.Add(c.AdsValueToDateTime());
                }
                return times.OrderByDescending(t => t).FirstOrDefault();
            }
        }
        public virtual bool LockedOut
        {
            get
            {
                return LockoutTime != null;

                //var date = LockoutTime;
                //bool matches = date != null && !date.Equals(ADS_NULL_TIME) && !date.Equals(DateTime.MinValue);
                //return matches;
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
                    UAC = UAC | ADS_UF_ACCOUNTDISABLE;
                }
                else if (!value && Disabled)
                {

                    UAC = UAC & ACCOUNT_ENABLE_MASK;

                }
            }
        }

        protected int UAC
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



        public bool Enabled { get => !Disabled; set => Disabled = !value; }
        public virtual DateTime? ExpireTime
        {
            get
            {
                
                //var com = GetProperty<object>("accountExpires");
                var time = GetDateTimeProperty("accountExpires");


                if (time != null)
                    time = time?.ToLocalTime();
                return time;
            }
            set
            {

                if (value == null)
                    value = CommonHelpers.ADS_NULL_TIME;
                SetProperty("accountExpires", value?.ToUniversalTime().ToFileTime().ToString());
            }
        }


        public DateTime? PasswordLastSet
        {
            get
            {
               
                return GetDateTimeProperty("pwdLastSet");
               // if (com == null) return null;
                //return com.AdsValueToDateTime();

            }

        }




    }
}
