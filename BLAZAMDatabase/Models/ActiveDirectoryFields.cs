using BLAZAM.Localization;

namespace BLAZAM.Database.Models
{

    public class ActiveDirectoryFields
    {



#pragma warning disable CA2211 // Non-constant fields should not be visible
        public static ActiveDirectoryField SN = new()
        {
            Id = 1,
            FieldName = "sn",
            DisplayName = Lang.Last_Name,
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField GivenName = new()
        {
            Id = 2,
            FieldName = "givenname",
            DisplayName = Lang.First_Name,
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField PhysicalDeliveryOffice = new()
        {
            Id = 3,
            FieldName = "physicalDeliveryOfficeName",
            DisplayName = Lang.Office,
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField EmployeeId = new()
        {
            Id = 4,
            FieldName = "employeeId",
            DisplayName = Lang.Employee_Id,
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField HomeDirectory = new()
        {
            Id = 5,
            FieldName = "homeDirectory",
            DisplayName = Lang.Home_Directory,
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField ScriptPath = new()
        {
            Id = 6,
            FieldName = "scriptPath",
            DisplayName = "Logon Script Path",
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField ProfilePath = new()
        {
            Id = 7,
            FieldName = "profilePath",
            DisplayName = Lang.Profile_Path,
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField HomePhone = new()
        {
            Id = 8,
            FieldName = "homePhone",
            DisplayName = "Home Phone Number",
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField StreetAddress = new()
        {
            Id = 9,
            FieldName = "streetAddress",
            DisplayName = Lang.Street_Address,
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField City = new()
        {
            Id = 10,
            FieldName = "l", // 'l' is the standard LDAP attribute name for Locality (City)
            DisplayName = Lang.City,
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField State = new()
        {
            Id = 11,
            FieldName = "st", // 'st' is the standard LDAP attribute name for State or Province
            DisplayName = Lang.State,
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField PostalCode = new()
        {
            Id = 12,
            FieldName = "postalCode",
            DisplayName = Lang.Zip_Code,
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField Site = new()
        {
            Id = 13,
            FieldName = "site", // Note: 'site' is not a standard AD attribute. Check if this is custom.
            DisplayName = Lang.Site,
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField Name = new()
        {
            Id = 14,
            FieldName = "name", // Often the same as CN (Canonical Name)
            DisplayName = Lang.Name,
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField SAMAccountName = new()
        {
            Id = 15,
            FieldName = "samaccountname", // Pre-Windows 2000 logon name
            DisplayName = Lang.Username,
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField ObjectSID = new()
        {
            Id = 16,
            FieldName = "objectSID", // Security Identifier
            DisplayName = "SID",
            FieldType = ActiveDirectoryFieldType.RawData // SID is binary data
        };

        public static ActiveDirectoryField Mail = new()
        {
            Id = 17,
            FieldName = "mail",
            DisplayName = "E-Mail Address",
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField Description = new()
        {
            Id = 18,
            FieldName = "description",
            DisplayName = Lang.Description,
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField DisplayName = new()
        {
            Id = 19,
            FieldName = "displayName",
            DisplayName = Lang.Display_Name,
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField DistinguishedName = new()
        {
            Id = 20,
            FieldName = "distinguishedName", // Unique identifier within the directory (DN)
            DisplayName = "Distinguished Name",
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField MemberOf = new()
        {
            Id = 21,
            FieldName = "memberOf", // List of groups the object belongs to
            DisplayName = "Member Of",
            FieldType = ActiveDirectoryFieldType.StringList // Multi-valued attribute
        };

        public static ActiveDirectoryField Company = new()
        {
            Id = 22,
            FieldName = "company",
            DisplayName = Lang.Company,
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField Title = new()
        {
            Id = 23,
            FieldName = "title", // Job title
            DisplayName = "Title",
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField UserPrincipalName = new()
        {
            Id = 24,
            FieldName = "userPrincipalName", // UPN (e.g., user@domain.com)
            DisplayName = "User Principal Name",
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField TelephoneNumber = new()
        {
            Id = 25,
            FieldName = "telephoneNumber", // Primary work phone number
            DisplayName = "Telephone Number",
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField POBox = new()
        {
            Id = 26,
            FieldName = "postOfficeBox",
            DisplayName = Lang.PO_Box,
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField CanonicalName = new()
        {
            Id = 27,
            FieldName = "cn", // Canonical Name, often the object's common name
            DisplayName = "Canonical Name",
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField HomeDrive = new()
        {
            Id = 28,
            FieldName = "homeDrive", // Drive letter for home directory mapping (e.g., H:)
            DisplayName = Lang.Home_Drive,
            FieldType = ActiveDirectoryFieldType.DriveLetter // Assuming custom type or validation needed
        };

        public static ActiveDirectoryField Department = new()
        {
            Id = 29,
            FieldName = "department",
            DisplayName = Lang.Department,
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField MiddleName = new()
        {
            Id = 30,
            FieldName = "middleName", // Often stored in the 'initials' attribute ('initials')
            DisplayName = Lang.Middle_Name,
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField Pager = new()
        {
            Id = 31,
            FieldName = "pager",
            DisplayName = "Pager",
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField OperatingSystem = new()
        {
            Id = 32,
            FieldName = "operatingSystemVersion", // For computer objects
            DisplayName = "OS",
            FieldType = ActiveDirectoryFieldType.Text
        };

        public static ActiveDirectoryField AccountExpires = new()
        {
            Id = 33,
            FieldName = "accountExpires", // Date when the account expires (FILETIME format)
            DisplayName = "Account Expiration",
            FieldType = ActiveDirectoryFieldType.Date, // Represents a date/time value
            PropertyName = "ExpireTime"
        };

        public static ActiveDirectoryField Manager = new()
        {
            Id = 34,
            FieldName = "manager", // Distinguished Name (DN) of the user's manager
            DisplayName = "Manager",
            FieldType = ActiveDirectoryFieldType.Text // Stored as DN string
        };

        public static ActiveDirectoryField Thumbnail = new()
        {
            Id = 35,
            FieldName = "thumbnail",
            DisplayName = "Photo",
            FieldType = ActiveDirectoryFieldType.RawData // Binary image data
        };

        public static ActiveDirectoryField LogOnTo = new()
        {
            Id = 36,
            FieldName = "userWorkstations", // List of computer names the user can log on to
            DisplayName = Lang.Log_On_To,
            FieldType = ActiveDirectoryFieldType.Text // Comma-separated string
        };

        public static ActiveDirectoryField LogonHours = new()
        {
            Id = 37,
            FieldName = "logonHours", // Binary data representing allowed logon times
            DisplayName = Lang.Logon_Hours,
            FieldType = ActiveDirectoryFieldType.RawData
        };

        public static ActiveDirectoryField GroupType = new()
        {
            Id = 38,
            FieldName = "groupType", // Defines group scope (Domain Local, Global, Universal) and type (Security, Distribution)
            DisplayName = "Group Type and Scope",
            FieldType = ActiveDirectoryFieldType.RawData // Integer value representing flags
        };

        public static ActiveDirectoryField Enabled = new()
        {
            Id = 39,
            FieldName = "userAccountControl", 
            DisplayName = Lang.Enabled,
            FieldType = ActiveDirectoryFieldType.Boolean,
            PropertyName = Lang.Enabled

        };

        public static ActiveDirectoryField Locked_Out = new()
        {
            Id = 40,
            FieldName = "lockoutTime",
            DisplayName = Lang.Locked_Out,
            FieldType = ActiveDirectoryFieldType.FileTime,
            PropertyName = "LockedOut"

        };

#pragma warning restore CA2211 // Non-constant fields should not be visible


    }
}
