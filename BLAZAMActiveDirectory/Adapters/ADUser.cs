using System.ComponentModel.DataAnnotations;
using System.Security.AccessControl;
using BLAZAM.ActiveDirectory.Data;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Models;
using BLAZAM.FileSystem;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using Microsoft.IdentityModel.Tokens;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class ADUser : AccountDirectoryAdapter, IADUser
    {

        public string? LogOnTo
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.LogOnTo.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.LogOnTo.FieldName, value);
            }
        }
        public LogonHours? LogonHours
        {
            get
            {
                var raw = GetAttribute<byte[]>(ActiveDirectoryFields.LogonHours.FieldName);
                var decoded = new LogonHours(raw);
                return decoded;
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.LogonHours.FieldName, value?.EncodeLogonHours());
            }
        }
        public virtual async Task<IJob> CommitChangesAsync(IJob? commitJob = null)
        {
            return await Task.Run(() =>
            {
                return CommitChanges(commitJob);
            });
        }
       

        [Required]
        public override string? DisplayName { get => base.DisplayName; set => base.DisplayName = value; }


        public List<FailedADLogonEvent> FailedLogonEvents => this.DomainControllerEventLogs.GetFailedLogonEvents(this, DateTime.UtcNow - TimeSpan.FromDays(5), DateTime.UtcNow);

        public string? HomeDirectory
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.HomeDirectory.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.HomeDirectory.FieldName, value);
                if (value == null || value == "") return;


                PostCommitSteps.Add(new JobStep("Create home directory", (JobStep step) =>
                {
                    return Directory.Impersonation.Run(() =>
                    {
                        if (HomeDirectory == null || HomeDirectory.IsNullOrEmpty()) return true;
                        var homeDirectory = new SystemDirectory(HomeDirectory);
                        if (!homeDirectory.Exists)
                            homeDirectory.EnsureCreated();
                        SetHomeDirectoryPermissions();
                        if (homeDirectory.Exists)
                            return true;
                        return false;

                    });
                }));
            }
        }

        /// <summary>
        /// Automatically called when changing <see cref="HomeDirectory"/>
        /// </summary>
        /// <remarks>Must be called under an identity context that has permission to make these changes</remarks>
        public void SetHomeDirectoryPermissions()
        {
            if (SAMAccountName == null) throw new AppException("Samaccount name is null while setting home directory");
            if (HomeDirectory == null) throw new AppException("HomeDirectory is null while setting home directory");
            FileSystemRights Rights;

            //What rights are we setting?

            Rights = FileSystemRights.FullControl;
            bool modified;
            InheritanceFlags none = InheritanceFlags.None;

            //set on dir itself
            FileSystemAccessRule accessRule = new(SAMAccountName, Rights, none, PropagationFlags.NoPropagateInherit, AccessControlType.Allow);
            DirectoryInfo dInfo = new(HomeDirectory);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.ModifyAccessRule(AccessControlModification.Set, accessRule, out modified);

            //Always allow objects to inherit on a directory 
            InheritanceFlags iFlags = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;


            //Add Access rule for the inheritance
            FileSystemAccessRule accessRule2 = new(SAMAccountName, Rights, iFlags, PropagationFlags.InheritOnly, AccessControlType.Allow);
            dSecurity.ModifyAccessRule(AccessControlModification.Add, accessRule2, out modified);

            dInfo.SetAccessControl(dSecurity);
        }
        public string? ScriptPath
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.ScriptPath.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.ScriptPath.FieldName, value);
            }
        }

        public override string? SAMAccountName
        {
            get => base.SAMAccountName;
            set
            {
                base.SAMAccountName = value;
                if (UserPrincipalName.IsNullOrEmpty())
                    UserPrincipalName = value + "@" + DbFactory.CreateDbContext().ActiveDirectorySettings.FirstOrDefault()?.FQDN;

                else
                    UserPrincipalName = value + "@" + UserPrincipalName?.Split("@")[1];
            }

        }

        public string? ProfilePath
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.ProfilePath.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.ProfilePath.FieldName, value);
            }
        }
        public string? HomeDrive
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.HomeDrive.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.HomeDrive.FieldName, value);
            }
        }
        public string? UserPrincipalName
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.UserPrincipalName.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.UserPrincipalName.FieldName, value);
            }
        }



    }
}
