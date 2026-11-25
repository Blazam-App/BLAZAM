using BLAZAM.Helpers;
using BLAZAM.Localization;

namespace BLAZAM.Database.Models
{
    public static class ActiveDirectoryFields
    {
        public const string SnFieldName = "sn";
        public const string GivenNameFieldName = "givenname";
        public const string PhysicalDeliveryOfficeNameFieldName = "physicalDeliveryOfficeName";
        public const string EmployeeIdFieldName = "employeeId";
        public const string HomeDirectoryFieldName = "homeDirectory";
        public const string ScriptPathFieldName = "scriptPath";
        public const string ProfilePathFieldName = "profilePath";
        public const string HomePhoneFieldName = "homePhone";
        public const string StreetAddressFieldName = "streetAddress";
        public const string CityFieldName = "l";
        public const string StateFieldName = "st";
        public const string ZipFieldName = "postalCode";
        public const string SiteFieldName = "site";
        public const string NameFieldName = "name";
        public const string SAMAccountNameFieldName = "samaccountname";
        public const string SIDFieldName = "objectSID";
        public const string EmailFieldName = "mail";
        public const string DescriptionFieldName = "description";
        public const string DisplayNameFieldName = "displayName";
        public const string DNFieldName = "distinguishedName";
        public const string MemberOfFieldName = "memberOf";
        public const string CompanyFieldName = "company";
        public const string TitleFieldName = "title";
        public const string UserPrincipalNameFieldName = "userPrincipalName";
        public const string TelephoneNumberFieldName = "telephoneNumber";
        public const string POBoxFieldName = "postOfficeBox";
        public const string CanonicalNameFieldName = "cn";
        public const string HomeDriveFieldName = "homeDrive";
        public const string DepartmentFieldName = "department";
        public const string MiddleNameFieldName = "middleName";
        public const string PagerFieldName = "pager";
        public const string OSFieldName = "operatingSystemVersion";
        public const string ExpireTimeFieldName = "accountExpires";
        public const string ManagerFieldName = "manager";
        public const string ThumbnailPhotoFieldName = "thumbnail";
        public const string LogOnToFieldName = "userWorkstations";
        public const string LogonHoursFieldName = "logonHours";
        public const string GroupTypeFieldName = "groupType";
        public const string GroupScopeFieldName = "groupType";
        public const string EnabledFieldName = "userAccountControl";
        public const string LockedOutFieldName = "lockoutTime";
        public const string OUFieldName = "ou";
        public const string LastChangedFieldName = "whenChanged";
        public const string LastLogonTimestampFieldName = "lastLogonTimestamp";
        public const string LapsPasswordFieldName = "msLAPS-Password";

        public static List<ActiveDirectoryField> Fields => typeof(ActiveDirectoryFields).GetStaticProperties<ActiveDirectoryField>();

        public static readonly ActiveDirectoryField SN = new()
        {
            Id = 1,
            FieldName = SnFieldName,
            DisplayName = Lang.Last_Name,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "Sn"
        };

        public static readonly ActiveDirectoryField GivenName = new()
        {
            Id = 2,
            FieldName = GivenNameFieldName,
            DisplayName = Lang.First_Name,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "GivenName"
        };

        public static readonly ActiveDirectoryField PhysicalDeliveryOffice = new()
        {
            Id = 3,
            FieldName = PhysicalDeliveryOfficeNameFieldName,
            DisplayName = Lang.Office,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "PhysicalDeliveryOfficeName"
        };

        public static readonly ActiveDirectoryField EmployeeId = new()
        {
            Id = 4,
            FieldName = EmployeeIdFieldName,
            DisplayName = Lang.Employee_Id,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "EmployeeId"
        };

        public static readonly ActiveDirectoryField HomeDirectory = new()
        {
            Id = 5,
            FieldName = HomeDirectoryFieldName,
            DisplayName = Lang.Home_Directory,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "HomeDirectory"
        };

        public static readonly ActiveDirectoryField ScriptPath = new()
        {
            Id = 6,
            FieldName = ScriptPathFieldName,
            DisplayName = "Logon Script Path",
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "ScriptPath"
        };

        public static readonly ActiveDirectoryField ProfilePath = new()
        {
            Id = 7,
            FieldName = ProfilePathFieldName,
            DisplayName = Lang.Profile_Path,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "ProfilePath"
        };

        public static readonly ActiveDirectoryField HomePhone = new()
        {
            Id = 8,
            FieldName = HomePhoneFieldName,
            DisplayName = "Home Phone Number",
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "HomePhone"
        };

        public static readonly ActiveDirectoryField StreetAddress = new()
        {
            Id = 9,
            FieldName = StreetAddressFieldName,
            DisplayName = Lang.Street_Address,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "StreetAddress"
        };

        public static readonly ActiveDirectoryField City = new()
        {
            Id = 10,
            FieldName = CityFieldName, // 'l' is the standard LDAP attribute name for Locality (City)
            DisplayName = Lang.City,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "City"
        };

        public static readonly ActiveDirectoryField State = new()
        {
            Id = 11,
            FieldName = StateFieldName, // 'st' is the standard LDAP attribute name for State or Province
            DisplayName = Lang.State,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "State"
        };

        public static readonly ActiveDirectoryField PostalCode = new()
        {
            Id = 12,
            FieldName = ZipFieldName,
            DisplayName = Lang.Zip_Code,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "Zip"
        };

        public static readonly ActiveDirectoryField Site = new()
        {
            Id = 13,
            FieldName = SiteFieldName, // Note: 'site' is not a standard AD attribute. Check if this is custom.
            DisplayName = Lang.Site,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "Site"
        };

        public static readonly ActiveDirectoryField Name = new()
        {
            Id = 14,
            FieldName = NameFieldName, // Often the same as CN (Canonical Name)
            DisplayName = Lang.Name,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "Name"
        };

        public static readonly ActiveDirectoryField SAMAccountName = new()
        {
            Id = 15,
            FieldName = SAMAccountNameFieldName, // Pre-Windows 2000 logon name
            DisplayName = Lang.Username,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "SAMAccountName"
        };

        public static readonly ActiveDirectoryField ObjectSID = new()
        {
            Id = 16,
            FieldName = SIDFieldName, // Security Identifier
            DisplayName = "SID",
            FieldType = ActiveDirectoryFieldType.RawData, // SID is binary data,
            PropertyName = "SID"
        };

        /// <summary>
        /// Email Address
        /// </summary>
        public static readonly ActiveDirectoryField Mail = new()
        {
            Id = 17,
            FieldName = EmailFieldName,
            DisplayName = "E-Mail Address",
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "Email"
        };

        public static readonly ActiveDirectoryField Description = new()
        {
            Id = 18,
            FieldName = DescriptionFieldName,
            DisplayName = Lang.Description,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "Description"
        };

        public static readonly ActiveDirectoryField DisplayName = new()
        {
            Id = 19,
            FieldName = DisplayNameFieldName,
            DisplayName = Lang.Display_Name,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "DisplayName"
        };

        public static readonly ActiveDirectoryField DistinguishedName = new()
        {
            Id = 20,
            FieldName = DNFieldName, // Unique identifier within the directory (DN)
            DisplayName = "Distinguished Name",
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "DN"
        };

        public static readonly ActiveDirectoryField MemberOf = new()
        {
            Id = 21,
            FieldName = MemberOfFieldName, // List of groups the object belongs to
            DisplayName = "Member Of",
            FieldType = ActiveDirectoryFieldType.StringList,
            PropertyName = "MemberOf"
        };

        public static readonly ActiveDirectoryField Company = new()
        {
            Id = 22,
            FieldName = CompanyFieldName,
            DisplayName = Lang.Company,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "Company"
        };

        public static readonly ActiveDirectoryField Title = new()
        {
            Id = 23,
            FieldName = TitleFieldName, // Job title
            DisplayName = "Title",
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "Title"
        };

        public static readonly ActiveDirectoryField UserPrincipalName = new()
        {
            Id = 24,
            FieldName = UserPrincipalNameFieldName, // UPN (e.g., user@domain.com)
            DisplayName = "User Principal Name",
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "UserPrincipalName"
        };

        /// <summary>
        /// Telephone Number (Not Home Phone)
        /// </summary>
        public static readonly ActiveDirectoryField TelephoneNumber = new()
        {
            Id = 25,
            FieldName = TelephoneNumberFieldName, // Primary work phone number
            DisplayName = "Telephone Number",
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "TelephoneNumber"
        };

        public static readonly ActiveDirectoryField POBox = new()
        {
            Id = 26,
            FieldName = POBoxFieldName,
            DisplayName = Lang.PO_Box,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "POBox"
        };

        public static readonly ActiveDirectoryField CanonicalName = new()
        {
            Id = 27,
            FieldName = CanonicalNameFieldName, // Canonical Name, often the object's common name
            DisplayName = "Canonical Name",
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "CanonicalName"
        };

        public static readonly ActiveDirectoryField HomeDrive = new()
        {
            Id = 28,
            FieldName = HomeDriveFieldName, // Drive letter for home directory mapping (e.g., H:)
            DisplayName = Lang.Home_Drive,
            FieldType = ActiveDirectoryFieldType.DriveLetter,
            PropertyName = "HomeDrive"
        };

        public static readonly ActiveDirectoryField Department = new()
        {
            Id = 29,
            FieldName = DepartmentFieldName,
            DisplayName = Lang.Department,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "Department"
        };

        public static readonly ActiveDirectoryField MiddleName = new()
        {
            Id = 30,
            FieldName = MiddleNameFieldName, // Often stored in the 'initials' attribute ('initials')
            DisplayName = Lang.Middle_Name,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "MiddleName"
        };

        public static readonly ActiveDirectoryField Pager = new()
        {
            Id = 31,
            FieldName = PagerFieldName,
            DisplayName = "Pager",
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "Pager"
        };

        public static readonly ActiveDirectoryField OperatingSystem = new()
        {
            Id = 32,
            FieldName = OSFieldName, // For computer objects
            DisplayName = "OS",
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "OS"
        };

        public static readonly ActiveDirectoryField AccountExpires = new()
        {
            Id = 33,
            FieldName = ExpireTimeFieldName, // Date when the account expires
            DisplayName = "Account Expiration",
            FieldType = ActiveDirectoryFieldType.FileTime,
            PropertyName = "ExpireTime"
        };

        public static readonly ActiveDirectoryField Manager = new()
        {
            Id = 34,
            FieldName = ManagerFieldName, // Distinguished Name (DN) of the user's manager
            DisplayName = "Manager",
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "Manager"
        };

        public static readonly ActiveDirectoryField Thumbnail = new()
        {
            Id = 35,
            FieldName = ThumbnailPhotoFieldName,
            DisplayName = "Photo",
            FieldType = ActiveDirectoryFieldType.RawData,
            PropertyName = "ThumbnailPhoto"
        };

        public static readonly ActiveDirectoryField LogOnTo = new()
        {
            Id = 36,
            FieldName = LogOnToFieldName, // List of computer names the user can log on to
            DisplayName = Lang.Log_On_To,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "LogOnTo"
        };

        public static readonly ActiveDirectoryField LogonHours = new()
        {
            Id = 37,
            FieldName = LogonHoursFieldName, // Binary data representing allowed logon times
            DisplayName = Lang.Logon_Hours,
            FieldType = ActiveDirectoryFieldType.RawData,
            PropertyName = "LogonHours"
        };

        public static readonly ActiveDirectoryField GroupType = new()
        {
            Id = 38,
            FieldName = GroupTypeFieldName, // Defines group scope (Domain Local, Global, Universal) and type (Security, Distribution)
            DisplayName = "Group Type",
            FieldType = ActiveDirectoryFieldType.RawData,
            PropertyName = "GroupType"
        };

        public static readonly ActiveDirectoryField GroupScope = new()
        {
            Id = 39,
            FieldName = GroupScopeFieldName, // Defines group scope (Domain Local, Global, Universal) and type (Security, Distribution)
            DisplayName = "Group Scope",
            FieldType = ActiveDirectoryFieldType.RawData,
            PropertyName = "GroupScope"
        };

        public static readonly ActiveDirectoryField Enabled = new()
        {
            Id = 40,
            FieldName = EnabledFieldName,
            DisplayName = Lang.Enabled,
            FieldType = ActiveDirectoryFieldType.Boolean,
            PropertyName = Lang.Enabled
        };

        public static readonly ActiveDirectoryField LockedOut = new()
        {
            Id = 41,
            FieldName = LockedOutFieldName,
            DisplayName = Lang.Locked_Out,
            FieldType = ActiveDirectoryFieldType.FileTime,
            PropertyName = "LockedOut"
        };

        public static readonly ActiveDirectoryField OU = new()
        {
            Id = 42,
            FieldName = OUFieldName,
            DisplayName = Lang.OU,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "OU"
        };

        public static readonly ActiveDirectoryField LastChanged = new()
        {
            Id = 43,
            FieldName = LastChangedFieldName,
            DisplayName = Lang.Last_Change,
            FieldType = ActiveDirectoryFieldType.Date,
            PropertyName = "LastChanged"
        };

        public static readonly ActiveDirectoryField LastLogonTimestamp = new()
        {
            Id = 44,
            FieldName = LastLogonTimestampFieldName,
            DisplayName = Lang.Last_Logon,
            FieldType = ActiveDirectoryFieldType.Date,
            PropertyName = "LastLogonTimestamp"
        };

        public static readonly ActiveDirectoryField LapsPassword = new()
        {
            Id = 45,
            FieldName = LapsPasswordFieldName,
            DisplayName = Lang.LAPS_Password,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "LapsPassword"
        };
    }
}