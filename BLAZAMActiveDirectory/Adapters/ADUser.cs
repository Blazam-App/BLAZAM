using BLAZAM.ActiveDirectory;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Database.Models;
using BLAZAM.FileSystem;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using BLAZAM.Logger;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Security;
using System.Security.AccessControl;
using System.Text;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class ADUser : AccountDirectoryAdapter, IADUser
    {
        public byte[]? ThumbnailPhoto
        {

            get
            {
                return GetProperty<byte[]>("thumbnailPhoto");
            }
            set
            {
                SetProperty("thumbnailPhoto", value);
            }
        }
        public string? Pager
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.Pager.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.Pager.FieldName, value);
            }
        }
        public string? GivenName
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.GivenName.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.GivenName.FieldName, value);
            }
        }
        public string? MiddleName
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.MiddleName.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.MiddleName.FieldName, value);
            }
        }
        public string? Surname
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.SN.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.SN.FieldName, value);
            }
        }

        [Required]
        public override string? DisplayName { get => base.DisplayName; set => base.DisplayName = value; }

        public string? Department
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.Department.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.Department.FieldName, value);
            }
        }
        public string? PhysicalDeliveryOfficeName
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.PhysicalDeliveryOffice.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.PhysicalDeliveryOffice.FieldName, value);
            }
        }
        public string? EmployeeId
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.EmployeeId.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.EmployeeId.FieldName, value);
            }
        }
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

               
                PostCommitSteps.Add(new Jobs.JobStep("Create home directory", (JobStep? step) =>
                {
                    return Directory.Impersonation.Run(() =>
                    {
                        if (HomeDirectory.IsNullOrEmpty()) return true;
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
            if (SamAccountName == null) throw new ApplicationException("Samaccount name is null while setting home directory");
            if (HomeDirectory == null) throw new ApplicationException("HomeDirectory is null while setting home directory");
            FileSystemRights Rights;

            //What rights are we setting?

            Rights = FileSystemRights.FullControl;
            bool modified;
            InheritanceFlags none = new InheritanceFlags();
            none = InheritanceFlags.None;

            //set on dir itself
            FileSystemAccessRule accessRule = new FileSystemAccessRule(SamAccountName, Rights, none, PropagationFlags.NoPropagateInherit, AccessControlType.Allow);
            DirectoryInfo dInfo = new DirectoryInfo(HomeDirectory);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.ModifyAccessRule(AccessControlModification.Set, accessRule, out modified);

            //Always allow objects to inherit on a directory 
            InheritanceFlags iFlags = new InheritanceFlags();
            iFlags = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;

            //Add Access rule for the inheritance
            FileSystemAccessRule accessRule2 = new FileSystemAccessRule(SamAccountName, Rights, iFlags, PropagationFlags.InheritOnly, AccessControlType.Allow);
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

        public string? Title
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.Title.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.Title.FieldName, value);
            }
        }

        public string? Company
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.Company.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.Company.FieldName, value);
            }
        }

        public string? TelephoneNumber
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.TelephoneNumber.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.TelephoneNumber.FieldName, value);
            }

        }

        public string? HomePhone
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.HomePhone.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.HomePhone.FieldName, value);
            }

        }
        public string? StreetAddress
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.StreetAddress.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.StreetAddress.FieldName, value);
            }
        }
        public string? POBox
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.POBox.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.POBox.FieldName, value);
            }
        }
        public string? City
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.City.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.City.FieldName, value);
            }
        }
        public string? State
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.State.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.State.FieldName, value);
            }
        }
        public string? Zip
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.PostalCode.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.PostalCode.FieldName, value);
            }
        }
        public string? Site
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.Site.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.Site.FieldName, value);
            }
        }



    }
}
