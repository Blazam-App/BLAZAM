using BLAZAM.Localization;

namespace BLAZAM.Database.Models
{

    public class ActiveDirectoryFields
    {



#pragma warning disable CA2211 // Non-constant fields should not be visible
        public static ActiveDirectoryField SN = new() { Id = 1, FieldName = "sn", DisplayName = Lang.Last_Name, FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField GivenName = new() { Id = 2, FieldName = "givenname", DisplayName = Lang.First_Name, FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField PhysicalDeliveryOffice = new() { Id = 3, FieldName = "physicalDeliveryOfficeName", DisplayName = Lang.Office, FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField EmployeeId = new() { Id = 4, FieldName = "employeeId", DisplayName = Lang.Employee_Id, FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField HomeDirectory = new() { Id = 5, FieldName = "homeDirectory", DisplayName = Lang.Home_Directory, FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField ScriptPath = new() { Id = 6, FieldName = "scriptPath", DisplayName = "Logon Script Path", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField ProfilePath = new() { Id = 7, FieldName = "profilePath", DisplayName = Lang.Profile_Path, FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField HomePhone = new() { Id = 8, FieldName = "homePhone", DisplayName = "Home Phone Number", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField StreetAddress = new() { Id = 9, FieldName = "streetAddress", DisplayName = Lang.Street_Address, FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField City = new() { Id = 10, FieldName = "l", DisplayName = Lang.City, FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField State = new() { Id = 11, FieldName = "st", DisplayName = Lang.State, FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField PostalCode = new() { Id = 12, FieldName = "postalCode", DisplayName = Lang.Zip_Code, FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField Site = new() { Id = 13, FieldName = "site", DisplayName = Lang.Site, FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField Name = new() { Id = 14, FieldName = "name", DisplayName = Lang.Name, FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField SAMAccountName = new() { Id = 15, FieldName = "samaccountname", DisplayName = Lang.Username, FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField ObjectSID = new() { Id = 16, FieldName = "objectSID", DisplayName = "SID", FieldType = ActiveDirectoryFieldType.RawData };
        public static ActiveDirectoryField Mail = new() { Id = 17, FieldName = "mail", DisplayName = "E-Mail Address", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField Description = new() { Id = 18, FieldName = "description", DisplayName = "Description", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField DisplayName = new() { Id = 19, FieldName = "displayName", DisplayName = "Display Name", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField DistinguishedName = new() { Id = 20, FieldName = "distinguishedName", DisplayName = "Distinguished Name", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField MemberOf = new() { Id = 21, FieldName = "memberOf", DisplayName = "Member Of", FieldType = ActiveDirectoryFieldType.StringList };
        public static ActiveDirectoryField Company = new() { Id = 22, FieldName = "company", DisplayName = "Company", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField Title = new() { Id = 23, FieldName = "title", DisplayName = "Title", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField UserPrincipalName = new() { Id = 24, FieldName = "userPrincipalName", DisplayName = "User Principal Name", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField TelephoneNumber = new() { Id = 25, FieldName = "telephoneNumber", DisplayName = "Telephone Number", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField POBox = new() { Id = 26, FieldName = "postOfficeBox", DisplayName = Lang.PO_Box, FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField CanonicalName = new() { Id = 27, FieldName = "cn", DisplayName = "Canonical Name", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField HomeDrive = new() { Id = 28, FieldName = "homeDrive", DisplayName = Lang.Home_Drive, FieldType = ActiveDirectoryFieldType.DriveLetter };
        public static ActiveDirectoryField Department = new() { Id = 29, FieldName = "department", DisplayName = Lang.Department, FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField MiddleName = new() { Id = 30, FieldName = "middleName", DisplayName = Lang.Middle_Name, FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField Pager = new() { Id = 31, FieldName = "pager", DisplayName = "Pager", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField OperatingSystem = new() { Id = 32, FieldName = "operatingSystemVersion", DisplayName = "OS", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField AccountExpires = new() { Id = 33, FieldName = "accountExpires", DisplayName = "Account Expiration", FieldType = ActiveDirectoryFieldType.Date };
        public static ActiveDirectoryField Manager = new() { Id = 34, FieldName = "manager", DisplayName = "Manager", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField Thumbnail = new() { Id = 35, FieldName = "thumbnail", DisplayName = "Photo", FieldType = ActiveDirectoryFieldType.RawData };
        public static ActiveDirectoryField LogOnTo = new() { Id = 36, FieldName = "userWorkstations", DisplayName = Lang.Log_On_To, FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField LogonHours = new() { Id = 37, FieldName = "logonHours", DisplayName = Lang.Logon_Hours, FieldType = ActiveDirectoryFieldType.RawData };
        public static ActiveDirectoryField GroupType = new() { Id = 38, FieldName = "groupType", DisplayName = "Group Type and Scope", FieldType = ActiveDirectoryFieldType.RawData };
        public static ActiveDirectoryField Enabled = new() { Id = 39, FieldName = "uac", DisplayName = Lang.Enabled, FieldType = ActiveDirectoryFieldType.Boolean };
        public static ActiveDirectoryField Locked_Out = new() { Id = 40, FieldName = "lockoutTime", DisplayName = Lang.Locked_Out, FieldType = ActiveDirectoryFieldType.FileTime };
#pragma warning restore CA2211 // Non-constant fields should not be visible


    }
}
