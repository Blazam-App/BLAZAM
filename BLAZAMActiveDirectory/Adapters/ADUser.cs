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
    public class ADUser : AccountDirectoryAdapter, IADUser
    {
        public byte[]? ThumbnailPhoto
        {

            get
            {
                return GetAttribute<byte[]>(ActiveDirectoryFields.Thumbnail.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.Thumbnail.FieldName, value);
                SetAttribute(ActiveDirectoryFields.Thumbnail.FieldName, value);
            }
        }
        public string? Pager
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.Pager.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.Pager.FieldName, value);
            }
        }
        public string? GivenName
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.GivenName.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.GivenName.FieldName, value);
            }
        }
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
        public string? MiddleName
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.MiddleName.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.MiddleName.FieldName, value);
            }
        }
        public string? Sn
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.SN.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.SN.FieldName, value);
            }
        }

        [Required]
        public override string? DisplayName { get => base.DisplayName; set => base.DisplayName = value; }

        public string? Department
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.Department.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.Department.FieldName, value);
            }
        }
        public string? PhysicalDeliveryOfficeName
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.PhysicalDeliveryOffice.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.PhysicalDeliveryOffice.FieldName, value);
            }
        }
        public string? EmployeeId
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.EmployeeId.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.EmployeeId.FieldName, value);
            }
        }
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
                return GetStringAttribute(ActiveDirectoryFields.ScriptPath.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.ScriptPath.FieldName, value);
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

        public string? Title
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.Title.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.Title.FieldName, value);
            }
        }

        public string? Company
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.Company.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.Company.FieldName, value);
            }
        }

        public string? TelephoneNumber
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.TelephoneNumber.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.TelephoneNumber.FieldName, value);
            }

        }

        public string? HomePhone
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.HomePhone.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.HomePhone.FieldName, value);
            }

        }
        public string? StreetAddress
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.StreetAddress.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.StreetAddress.FieldName, value);
            }
        }
        public string? POBox
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.POBox.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.POBox.FieldName, value);
            }
        }
        public string? City
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.City.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.City.FieldName, value);
            }
        }
        public string? State
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.State.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.State.FieldName, value);
            }
        }
        public string? Zip
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.PostalCode.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.PostalCode.FieldName, value);
            }
        }
        public string? Site
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.Site.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.Site.FieldName, value);
            }
        }



    }
}
