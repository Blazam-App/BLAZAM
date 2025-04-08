using BLAZAM.ActiveDirectory.Data;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Exceptions;
using BLAZAM.Database.Models;
using BLAZAM.FileSystem;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Security.AccessControl;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class ADUser : ADContact, IADUser
    {
      
        public string? LogOnTo
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.LogOnTo.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.LogOnTo.FieldName, value);
            }
        }
        public LogonHours? LogonHours
        {
            get
            {
                var raw = GetProperty<byte[]>(ActiveDirectoryFields.LogonHours.FieldName);
                var decoded = new LogonHours(raw);
                return decoded;
            }
            set
            {
                SetProperty(ActiveDirectoryFields.LogonHours.FieldName, value?.EncodeLogonHours());
            }
        }
       

        [Required]
        public override string? DisplayName { get => base.DisplayName; set => base.DisplayName = value; }

       
        public List<FailedADLogonEvent> FailedLogonEvents => this.DomainControllerEventLogs.GetFailedLogonEvents(this, DateTime.UtcNow - TimeSpan.FromDays(5), DateTime.UtcNow);

        public string? HomeDirectory
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.HomeDirectory.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.HomeDirectory.FieldName, value);
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
            if (SamAccountName == null) throw new AppException("Samaccount name is null while setting home directory");
            if (HomeDirectory == null) throw new AppException("HomeDirectory is null while setting home directory");
            FileSystemRights Rights;

            //What rights are we setting?

            Rights = FileSystemRights.FullControl;
            bool modified;
            InheritanceFlags none = InheritanceFlags.None;

            //set on dir itself
            FileSystemAccessRule accessRule = new(SamAccountName, Rights, none, PropagationFlags.NoPropagateInherit, AccessControlType.Allow);
            DirectoryInfo dInfo = new(HomeDirectory);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.ModifyAccessRule(AccessControlModification.Set, accessRule, out modified);

            //Always allow objects to inherit on a directory 
            InheritanceFlags iFlags = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;


            //Add Access rule for the inheritance
            FileSystemAccessRule accessRule2 = new(SamAccountName, Rights, iFlags, PropagationFlags.InheritOnly, AccessControlType.Allow);
            dSecurity.ModifyAccessRule(AccessControlModification.Add, accessRule2, out modified);

            dInfo.SetAccessControl(dSecurity);
        }
        public string? ScriptPath
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.ScriptPath.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.ScriptPath.FieldName, value);
            }
        }

        public override string? SamAccountName
        {
            get => base.SamAccountName;
            set
            {
                base.SamAccountName = value;
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
                return GetStringProperty(ActiveDirectoryFields.ProfilePath.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.ProfilePath.FieldName, value);
            }
        }
        public string? HomeDrive
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.HomeDrive.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.HomeDrive.FieldName, value);
            }
        }
        public string? UserPrincipalName
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.UserPrincipalName.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.UserPrincipalName.FieldName, value);
            }
        }

     



    }
}
