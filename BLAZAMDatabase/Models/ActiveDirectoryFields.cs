using BLAZAM.Database.Models.Permissions;
using BLAZAM.Helpers;
using BLAZAM.Localization;

namespace BLAZAM.Database.Models
{

    public static class ActiveDirectoryFields
    {
        public static List<ActiveDirectoryField> Fields => typeof(ActiveDirectoryFields).GetStaticProperties<ActiveDirectoryField>();

        public static readonly ActiveDirectoryField SN = new()
        {
            Id = 1,
            FieldName = "sn",
            DisplayName = Lang.Last_Name,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "Sn"
        };

        public static readonly ActiveDirectoryField GivenName = new()
        {
            Id = 2,
            FieldName = "givenname",
            DisplayName = Lang.First_Name,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "GivenName"
        };

        public static readonly ActiveDirectoryField PhysicalDeliveryOffice = new()
        {
            Id = 3,
            FieldName = "physicalDeliveryOfficeName",
            DisplayName = Lang.Office,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "PhysicalDeliveryOfficeName"
        };

        public static readonly ActiveDirectoryField EmployeeId = new()
        {
            Id = 4,
            FieldName = "employeeId",
            DisplayName = Lang.Employee_Id,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "EmployeeId"
        };

        public static readonly ActiveDirectoryField HomeDirectory = new()
        {
            Id = 5,
            FieldName = "homeDirectory",
            DisplayName = Lang.Home_Directory,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "HomeDirectory"
        };

        public static readonly ActiveDirectoryField ScriptPath = new()
        {
            Id = 6,
            FieldName = "scriptPath",
            DisplayName = "Logon Script Path",
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "ScriptPath"
        };

        public static readonly ActiveDirectoryField ProfilePath = new()
        {
            Id = 7,
            FieldName = "profilePath",
            DisplayName = Lang.Profile_Path,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "ProfilePath"
        };

        public static readonly ActiveDirectoryField HomePhone = new()
        {
            Id = 8,
            FieldName = "homePhone",
            DisplayName = "Home Phone Number",
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "HomePhone"
        };

        public static readonly ActiveDirectoryField StreetAddress = new()
        {
            Id = 9,
            FieldName = "streetAddress",
            DisplayName = Lang.Street_Address,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "StreetAddress"
        };

        public static readonly ActiveDirectoryField City = new()
        {
            Id = 10,
            FieldName = "l", // 'l' is the standard LDAP attribute name for Locality (City)
            DisplayName = Lang.City,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "City"

        };

        public static readonly ActiveDirectoryField State = new()
        {
            Id = 11,
            FieldName = "st", // 'st' is the standard LDAP attribute name for State or Province
            DisplayName = Lang.State,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "State"
        };

        public static readonly ActiveDirectoryField PostalCode = new()
        {
            Id = 12,
            FieldName = "postalCode",
            DisplayName = Lang.Zip_Code,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "Zip"
        };

        public static readonly ActiveDirectoryField Site = new()
        {
            Id = 13,
            FieldName = "site", // Note: 'site' is not a standard AD attribute. Check if this is custom.
            DisplayName = Lang.Site,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "Site"
        };

        public static readonly ActiveDirectoryField Name = new()
        {
            Id = 14,
            FieldName = "name", // Often the same as CN (Canonical Name)
            DisplayName = Lang.Name,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "Name"
        };

        public static readonly ActiveDirectoryField SAMAccountName = new()
        {
            Id = 15,
            FieldName = "samaccountname", // Pre-Windows 2000 logon name
            DisplayName = Lang.Username,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "SAMAccountName"
        };

        public static readonly ActiveDirectoryField ObjectSID = new()
        {
            Id = 16,
            FieldName = "objectSID", // Security Identifier
            DisplayName = "SID",
            FieldType = ActiveDirectoryFieldType.RawData, // SID is binary data,
            PropertyName = "SID"
        };

        public static readonly ActiveDirectoryField Mail = new()
        {
            Id = 17,
            FieldName = "mail",
            DisplayName = "E-Mail Address",
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "Email"
        };

        public static readonly ActiveDirectoryField Description = new()
        {
            Id = 18,
            FieldName = "description",
            DisplayName = Lang.Description,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "Description"
        };

        public static readonly ActiveDirectoryField DisplayName = new()
        {
            Id = 19,
            FieldName = "displayName",
            DisplayName = Lang.Display_Name,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "DisplayName"
        };

        public static readonly ActiveDirectoryField DistinguishedName = new()
        {
            Id = 20,
            FieldName = "distinguishedName", // Unique identifier within the directory (DN)
            DisplayName = "Distinguished Name",
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "DN"
        };

        public static readonly ActiveDirectoryField MemberOf = new()
        {
            Id = 21,
            FieldName = "memberOf", // List of groups the object belongs to
            DisplayName = "Member Of",
            FieldType = ActiveDirectoryFieldType.StringList,
            PropertyName = "MemberOf"

        };

        public static readonly ActiveDirectoryField Company = new()
        {
            Id = 22,
            FieldName = "company",
            DisplayName = Lang.Company,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "Company"
        };

        public static readonly ActiveDirectoryField Title = new()
        {
            Id = 23,
            FieldName = "title", // Job title
            DisplayName = "Title",
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "Title"
        };

        public static readonly ActiveDirectoryField UserPrincipalName = new()
        {
            Id = 24,
            FieldName = "userPrincipalName", // UPN (e.g., user@domain.com)
            DisplayName = "User Principal Name",
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "UserPrincipalName"
        };

        public static readonly ActiveDirectoryField TelephoneNumber = new()
        {
            Id = 25,
            FieldName = "telephoneNumber", // Primary work phone number
            DisplayName = "Telephone Number",
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "TelephoneNumber"
        };

        public static readonly ActiveDirectoryField POBox = new()
        {
            Id = 26,
            FieldName = "postOfficeBox",
            DisplayName = Lang.PO_Box,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "POBox"
        };

        public static readonly ActiveDirectoryField CanonicalName = new()
        {
            Id = 27,
            FieldName = "cn", // Canonical Name, often the object's common name
            DisplayName = "Canonical Name",
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "CanonicalName"
        };

        public static readonly ActiveDirectoryField HomeDrive = new()
        {
            Id = 28,
            FieldName = "homeDrive", // Drive letter for home directory mapping (e.g., H:)
            DisplayName = Lang.Home_Drive,
            FieldType = ActiveDirectoryFieldType.DriveLetter,
            PropertyName = "HomeDrive"
        };

        public static readonly ActiveDirectoryField Department = new()
        {
            Id = 29,
            FieldName = "department",
            DisplayName = Lang.Department,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "Department"
        };

        public static readonly ActiveDirectoryField MiddleName = new()
        {
            Id = 30,
            FieldName = "middleName", // Often stored in the 'initials' attribute ('initials')
            DisplayName = Lang.Middle_Name,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "MiddleName"
        };

        public static readonly ActiveDirectoryField Pager = new()
        {
            Id = 31,
            FieldName = "pager",
            DisplayName = "Pager",
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "Pager"
        };

        public static readonly ActiveDirectoryField OperatingSystem = new()
        {
            Id = 32,
            FieldName = "operatingSystemVersion", // For computer objects
            DisplayName = "OS",
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "OS"
        };

        public static readonly ActiveDirectoryField AccountExpires = new()
        {
            Id = 33,
            FieldName = "accountExpires", // Date when the account expires
            DisplayName = "Account Expiration",
            FieldType = ActiveDirectoryFieldType.FileTime, 
            PropertyName = "ExpireTime"
        };

        public static readonly ActiveDirectoryField Manager = new()
        {
            Id = 34,
            FieldName = "manager", // Distinguished Name (DN) of the user's manager
            DisplayName = "Manager",
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "Manager"
        };

        public static readonly ActiveDirectoryField Thumbnail = new()
        {
            Id = 35,
            FieldName = "thumbnail",
            DisplayName = "Photo",
            FieldType = ActiveDirectoryFieldType.RawData,
            PropertyName = "ThumbnailPhoto"
        };

        public static readonly ActiveDirectoryField LogOnTo = new()
        {
            Id = 36,
            FieldName = "userWorkstations", // List of computer names the user can log on to
            DisplayName = Lang.Log_On_To,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "LogOnTo"
        };

        public static readonly ActiveDirectoryField LogonHours = new()
        {
            Id = 37,
            FieldName = "logonHours", // Binary data representing allowed logon times
            DisplayName = Lang.Logon_Hours,
            FieldType = ActiveDirectoryFieldType.RawData,
            PropertyName = "LogonHours"
        };

        public static readonly ActiveDirectoryField GroupType = new()
        {
            Id = 38,
            FieldName = "groupType", // Defines group scope (Domain Local, Global, Universal) and type (Security, Distribution)
            DisplayName = "Group Type",
            FieldType = ActiveDirectoryFieldType.RawData,
            PropertyName = "GroupType"
        };
        public static readonly ActiveDirectoryField GroupScope = new()
        {
            Id = 39,
            FieldName = "groupType", // Defines group scope (Domain Local, Global, Universal) and type (Security, Distribution)
            DisplayName = "Group Scope",
            FieldType = ActiveDirectoryFieldType.RawData,
            PropertyName = "GroupScope"
        };
        public static readonly ActiveDirectoryField Enabled = new()
        {
            Id = 40,
            FieldName = "userAccountControl", 
            DisplayName = Lang.Enabled,
            FieldType = ActiveDirectoryFieldType.Boolean,
            PropertyName = Lang.Enabled

        };

        public static readonly ActiveDirectoryField LockedOut = new()
        {
            Id = 41,
            FieldName = "lockoutTime",
            DisplayName = Lang.Locked_Out,
            FieldType = ActiveDirectoryFieldType.Boolean,
            PropertyName = "LockedOut"

        };

        public static readonly ActiveDirectoryField OU = new()
        {
            Id = 42,
            FieldName = "ou",
            DisplayName = Lang.OU,
            FieldType = ActiveDirectoryFieldType.Text,
            PropertyName = "OU"

        };

        public static readonly ActiveDirectoryField LastChanged = new()
        {
            Id = 43,
            FieldName = "whenChanged",
            DisplayName = Lang.Last_Change,
            FieldType = ActiveDirectoryFieldType.Date,
            PropertyName = "LastChanged"

        };

        public static readonly ActiveDirectoryField LastLogonTimestamp = new()
        {
            Id = 44,
            FieldName = "lastLogonTimestamp",
            DisplayName = Lang.Last_Logon,
            FieldType = ActiveDirectoryFieldType.Date,
            PropertyName = "LastLogonTimestamp"

        };



    }
}
