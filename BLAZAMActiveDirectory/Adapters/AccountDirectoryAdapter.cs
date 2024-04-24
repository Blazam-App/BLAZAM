using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using BLAZAM.Logger;
using System.Data;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Security;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class AccountDirectoryAdapter : GroupableDirectoryAdapter, IAccountDirectoryAdapter
    {
        const int ADS_UF_ACCOUNTDISABLE = 0x0002;
        const int ADS_UF_PASSWD_NOTREQD = 0x0020;
        const int ADS_UF_PASSWD_CANT_CHANGE = 0x0040;
        const int ADS_UF_NORMAL_ACCOUNT = 0x0200;
        const int ADS_UF_DONT_EXPIRE_PASSWD = 0x10000;
        const int PASSWD_NOTREQD_MASK = 0xFFDF;

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
                    if (c is DateTime)
                    {
                        times.Add(c.AdsValueToDateTime());
                    }
                }
                return times.OrderByDescending(t => t).FirstOrDefault();
            }
        }
        public virtual bool LockedOut
        {
            get
            {
                return LockoutTime != null;
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
        public virtual bool PasswordNotRequired 
        {
            get
            {

                try
                {
                    return (UAC & ADS_UF_PASSWD_NOTREQD) == ADS_UF_PASSWD_NOTREQD;
                }
                catch
                {
                    // handle NullReferenceException
                }
                return true;
            }
            set
            {
                if (value && !PasswordNotRequired)
                {
                    UAC = UAC | ADS_UF_PASSWD_NOTREQD;
                }
                else if (!value && PasswordNotRequired)
                {

                    UAC = UAC & PASSWD_NOTREQD_MASK;

                }
            }
        }
        protected int UAC
        {
            get
            {
                var uacRaw= Convert.ToInt32(GetProperty<object>("userAccountControl"));
                if(uacRaw == 0)
                {
                    UAC = ADS_UF_NORMAL_ACCOUNT | ADS_UF_PASSWD_NOTREQD;
                    return ADS_UF_NORMAL_ACCOUNT | ADS_UF_PASSWD_NOTREQD;
                }
                return uacRaw;
            }
            set
            {
              //  PostCommitSteps.Add(new("Set UAC", (step) => {
                    SetProperty("userAccountControl", value);

                //    return true;
              //  }));
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



        public bool Enabled
        {
            get => !Disabled;
            set => Disabled = !value;
        }
        public virtual DateTime? ExpireTime
        {
            get
            {
                return GetDateTimeProperty("accountExpires");
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
            }

        }


        public SecureString? NewPassword { get; set; }



        public bool SetPassword(SecureString password, bool requireChange = false)
        {
            if (SamAccountName == null) throw new ApplicationException("samaccount name not found!");
            if (DirectorySettings == null) throw new ApplicationException("Directory settings not found when trying to change directory user password");

            var directoryPassword = DirectorySettings.Password.Decrypt();
            if (directoryPassword == null) return false;


            try
            {
                //var portOpen = NetworkTools.IsPortOpen(DirectorySettings.ServerAddress, 464);
                try
                {
                    Invoke("SetPassword", new[] { password.ToPlainText() });
                    return true;
                }catch (Exception ex)
                {
                    Loggers.ActiveDirectryLogger.Warning("Could not set password via Invoke {@Error}", ex);
                    //The following works outside the domain but may have issues with certs
                    using (PrincipalContext pContext = new PrincipalContext(
                        ContextType.Domain,
                        DirectorySettings.ServerAddress + ":" + DirectorySettings.ServerPort,
                        DirectorySettings.Username + "@" + DirectorySettings.FQDN,
                        directoryPassword
                        ))
                    {


                        UserPrincipal up = UserPrincipal.FindByIdentity(pContext, SamAccountName);
                        if (up != null)
                        {
                            up.SetPassword(password.ToPlainText());
                            if (requireChange)
                                up.ExpirePasswordNow();
                            if (NewEntry)
                                up.PasswordNotRequired = false;
                            up.Save();

                        }
                    }


                    return true;
                }
     
            }
            catch (Exception ex)
            {

                Loggers.ActiveDirectryLogger.Error("Error setting entry password {@Error}", ex);
                if (!Debugger.IsAttached)
                    throw new ApplicationException("Unable to set password", ex);
                else return true;
            }
            
        }

        public void StagePasswordChange(SecureString newPassword, bool requireChange = false)
        {
            NewPassword = newPassword;
            PostCommitSteps.Add(new JobStep("Set Password", (JobStep? step) =>
            {
                var pass = NewPassword;
                NewPassword = null;
                return SetPassword(pass, requireChange);
            }));


        }




    }
}
