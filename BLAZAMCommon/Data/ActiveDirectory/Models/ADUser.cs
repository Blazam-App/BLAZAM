using BLAZAM.Common.Data.ActiveDirectory;
using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Models.Database;
using BLAZAM.Common.Models.Database.Permissions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.AccountManagement;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Security.AccessControl;
using System.Text;

namespace BLAZAM.Common.Data.ActiveDirectory.Models
{
    public class ADUser : GroupableDirectoryModel, IADUser
    {

        public SecureString NewPassword { get; set; }
        public bool SetPassword(SecureString password, bool requireChange = false)
        {

            try
            {


                using (PrincipalContext pContext = new PrincipalContext(ContextType.Domain, DirectorySettings.FQDN, DirectorySettings.Username, Directory.Encryption.DecryptObject<string>(DirectorySettings.Password)))
                {
                    UserPrincipal up = UserPrincipal.FindByIdentity(pContext, SamAccountName);
                    if (up != null)
                    {
                        up.SetPassword(password.ToPlainText());
                        if (requireChange)
                            up.ExpirePasswordNow();

                        up.Save();

                    }
                }


                return true;
            }
            catch
            {
                return false;
            }

        }

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
        /// <summary>
        /// Users require a display name
        /// </summary>
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
                CommitActions.Add(() =>
                {
                    return WindowsImpersonation.Run(() =>
                    {
                        if (HomeDirectory.IsNullOrEmpty()) return true;

                        if (!System.IO.Directory.Exists(HomeDirectory))
                            System.IO.Directory.CreateDirectory(value);
                        SetHomeDirectoryPermissions();
                        if (System.IO.Directory.Exists(HomeDirectory))
                            return true;
                        return false;

                    });
                });
            }
        }

        public void StagePasswordChange(SecureString newPassword, bool requireChange = false)
        {
            CommitActions.Add(() =>
            {
                return SetPassword(newPassword, requireChange);
            });
        }
        /// <summary>
        /// Automatically called when changing <see cref="HomeDirectory"/>
        /// </summary>
        public void SetHomeDirectoryPermissions()
        {

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
        public DateTime? PasswordLastSet
        {
            get
            {
                var com = GetProperty<object>("pwdLastSet");
                return com.AdsValueToDateTime();

            }

        }

        public override string? SamAccountName
        {
            get => base.SamAccountName;
            set
            {
                base.SamAccountName = value;
                if (UserPrincipalName == null)
                    UserPrincipalName = value + "@" + Context.ActiveDirectorySettings.FirstOrDefault().FQDN;

                else
                    UserPrincipalName = value + "@" + UserPrincipalName.Split("@")[1];
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
        public string? Street
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.Street.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.Street.FieldName, value);
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

        public List<PermissionDelegate> PermissionDelegates { get; set; } = new List<PermissionDelegate>();

        public List<PermissionMap> PermissionMappings { get; set; } = new List<PermissionMap>();

        public bool HasUserPrivilege
        {
            get
            {
                return HasObjectReadPermissions(ObjectType);

            }

        }
        public bool HasCreateUserPrivilege
        {
            get
            {
                return HasObjectCreatePermissions(ObjectType);

            }

        }
        public bool HasGroupPrivilege
        {
            get
            {
                return HasObjectReadPermissions(ActiveDirectoryObjectType.Group);

            }
        }
        public bool HasCreateGroupPrivilege
        {
            get
            {
                return HasObjectCreatePermissions(ActiveDirectoryObjectType.Group);

            }

        }
        public bool HasOUPrivilege
        {
            get
            {
                return HasObjectReadPermissions(ActiveDirectoryObjectType.OU);

            }
        }
        public bool HasCreateOUPrivilege
        {
            get
            {
                return HasObjectCreatePermissions(ActiveDirectoryObjectType.OU);

            }

        }
        public bool HasComputerPrivilege
        {
            get
            {
                return HasObjectReadPermissions(ActiveDirectoryObjectType.Computer);

            }
        }

        private bool HasObjectReadPermissions(ActiveDirectoryObjectType objectType)
        {
            return PermissionMappings.Any(
                       m => m.AccessLevels.Any(
                           a => a.ObjectMap.Any(
                               o => o.ObjectType == objectType && o.ObjectAccessLevel.Level > ObjectAccessLevels.Deny.Level)
                           )
                       );
        }
        private bool HasObjectCreatePermissions(ActiveDirectoryObjectType objectType)
        {
            return PermissionMappings.Any(
                        m => m.AccessLevels.Any(
                            a => a.ObjectMap.Any(
                                o => o.ObjectType == objectType && o.ObjectAccessLevel.Level > ObjectAccessLevels.Deny.Level) &&
                                a.ActionMap.Any(am => am.ObjectType == objectType &&
                                am.ObjectAction.ActionAccessFlagId == ActionAccessFlags.Create.ActionAccessFlagId)
                            )
                        );
        }

    }
}
