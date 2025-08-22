using System.Data;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Security;
using BLAZAM.ActiveDirectory.Data;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using BLAZAM.Logger;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class AccountDirectoryAdapter : ADContact, IAccountDirectoryAdapter
    {
        private const int ADS_UF_ACCOUNTDISABLE = 0x0002;
        private const int ADS_UF_PASSWD_NOTREQD = 0x0020;
        private const int ADS_UF_PASSWD_CANT_CHANGE = 0x0040;
        private const int ADS_UF_NORMAL_ACCOUNT = 0x0200;
        private const int ADS_UF_DONT_EXPIRE_PASSWD = 0x10000;
        private const string pwdLastSet = "pwdLastSet";

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

        public virtual bool CanSetPassword { get => HasActionPermission(ObjectActions.SetPassword); }

        public virtual bool CanEnable { get => HasActionPermission(ObjectActions.Enable); }

        public virtual bool CanDisable { get => HasActionPermission(ObjectActions.Disable); }

        public virtual bool CanUnlock { get => HasActionPermission(ObjectActions.Unlock); }



        public bool CanSearchDisabled
        {
            get
            {
                if (CurrentUser?.IsSuperAdmin == true) return true;
                return CurrentUser?.PermissionMappings.Any(pm => pm.AccessLevels.Any(al => al.ObjectMap.Any(om => om.ObjectType == ObjectType && om.AllowDisabled))) == true;

            }
        }

        public virtual DateTime? LockoutTime
        {
            get
            {
                return GetDateTimeAttribute("lockoutTime");

            }
            set
            {
                if (value != null)
                {
                    SetAttribute("lockoutTime", DateTime.UtcNow);
                }
                else
                {

                    SetAttribute("lockoutTime", 0);
                }
            }
        }

        public virtual DateTime? LastLogonTimestamp
        {
            get
            {
                return GetDateTimeAttribute("lastLogonTimestamp");

            }
        }
        public virtual DateTime? PasswordExpirationTime
        {
            get
            {
                return GetDateTimeAttribute("msDS-UserPasswordExpiryTimeComputed");

            }
        }

        public DateTime? LastLogonTime
        {
            get
            {
                var coms = GetNonReplicatedProperty<object>("lastLogon");
                List<DateTime?> times = new();
                foreach (var c in coms)
                {
                    if (c is DateTime dt)
                    {
                        times.Add(dt);
                    }
                    else
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
                    LockoutTime = DateTime.UtcNow;
                }
                else
                {

                    LockoutTime = null;
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

                    UAC = UAC & ~ADS_UF_ACCOUNTDISABLE;

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
                    //Ignore error
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

                    UAC = UAC & ~ADS_UF_PASSWD_NOTREQD;

                }
            }
        }
        protected int UAC
        {
            get
            {
                var uacRaw = Convert.ToInt32(GetAttribute<object>("userAccountControl"));
                if (uacRaw == 0)
                {
                    UAC = ADS_UF_NORMAL_ACCOUNT | ADS_UF_PASSWD_NOTREQD;
                    return ADS_UF_NORMAL_ACCOUNT | ADS_UF_PASSWD_NOTREQD;
                }
                return uacRaw;
            }
            set
            {
                SetAttribute("userAccountControl", value);
            }
        }

        protected DomainControllerEventLogReader? DomainControllerEventLogs;


        public override void Parse(IActiveDirectoryContext directory, DirectoryEntry? directoryEntry = null, SearchResult? searchResult = null)
        {
            base.Parse(directory, directoryEntry, searchResult);
            DomainControllerEventLogs = new DomainControllerEventLogReader(directory);
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
                return GetDateTimeAttribute("accountExpires");
            }
            set
            {
                SetFileTimeAttribute("accountExpires", value);
            }
        }


        public void StageRequirePasswordChange(bool requireChange)
        {
            PostCommitSteps.Add(new JobStep("Require Password Change", (JobStep step) =>
            {

                RequirePasswordChange = requireChange;
                return true;
            }));

        }
        public virtual bool RequirePasswordChange
        {
            get
            {
                if (PasswordLastSet == null) return true;
                return PasswordLastSet == DateTime.MinValue;
            }
            set
            {
                if (value)
                {
                    PasswordLastSet = null;
                }
                else
                {


                    PasswordLastSet = DateTime.Now;


                }

            }
        }
        public DateTime? PasswordLastSet
        {
            get
            {

                try
                {
                    var dateTime = GetDateTimeAttribute(pwdLastSet)?.AdsValueToDateTime();
                    if (dateTime.HasValue)
                    {
                        return dateTime.Value;

                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        var rawValue = GetAttribute<Int32>(pwdLastSet);
                        if (rawValue == -1)
                        {
                            return DateTime.UtcNow;
                        }
                        if (rawValue == 0)
                        {
                            return null;
                        }
                    }
                    catch (Exception ex2)
                    {
                        //Ignore conversion errors during runtime
                    }
                }
                return null;
            }
            set
            {
                if (value == null)
                    SetAttribute(pwdLastSet, 0);
                else
                    SetAttribute(pwdLastSet, -1);

            }

        }


        public SecureString? NewPassword { get; set; }



        public bool SetPassword(SecureString password, bool requireChange = false)
        {
            if (SAMAccountName == null) throw new AppException("samaccount name not found!");
            if (DirectorySettings == null) throw new AppException("Directory settings not found when trying to change directory user password");

            var directoryPassword = DirectorySettings.Password.Decrypt().ToSecureString();
            if (directoryPassword == null) return false;

            try
            {
                if (TryInvokeSetPassword(password))
                    return true;
                if (OperatingSystem.IsWindows())
                {
                    // If we are on Windows, we can use the PrincipalContext to set the password
                    return TryPrincipalContextSetPassword(password, requireChange, directoryPassword);
                }

            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Error(ex, "Error setting entry password");
                if (!Debugger.IsAttached)
                    throw new AppException("Unable to set password", ex);
            }
            return false;
        }

        private bool TryInvokeSetPassword(SecureString password)
        {
            try
            {
                Invoke("SetPassword", new[] { password.ToPlainText() });
                return true;
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Warning(ex, "Could not set password via Invoke");
                return false;
            }
        }

        private bool TryPrincipalContextSetPassword(SecureString password, bool requireChange, SecureString directoryPassword)
        {
            if (DirectorySettings == null)
                throw new AppException("Directory settings not found when trying to change directory user password");
            using (PrincipalContext pContext = new(
                ContextType.Domain,
                DirectorySettings.ServerAddress + ":" + DirectorySettings.ServerPort,
                DirectorySettings.Username + "@" + DirectorySettings.FQDN,
                directoryPassword.ToPlainText()
            ))
            {
                UserPrincipal up = UserPrincipal.FindByIdentity(pContext, SAMAccountName);
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

        public void StagePasswordChange(SecureString newPassword, bool requireChange = false)
        {
            NewPassword = newPassword;
            PostCommitSteps.Add(new JobStep("Set Password", (JobStep step) =>
            {
                var pass = NewPassword;
                NewPassword = null;
                return SetPassword(pass, requireChange);
            }));


        }



    }
}
