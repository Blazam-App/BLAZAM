using BLAZAM.Common.Models.Database;
using System.Reflection;

namespace BLAZAM.Common.Models.Database
{
    
    public class ActiveDirectoryFields
    {



#pragma warning disable CA2211 // Non-constant fields should not be visible
        public static ActiveDirectoryField SN = new ActiveDirectoryField() { Id = 1, FieldName = "sn", DisplayName ="Last Name",FieldType=ActiveDirectoryFieldType.Text};
        public static ActiveDirectoryField GivenName = new ActiveDirectoryField() { Id = 2, FieldName = "givenname", DisplayName ="First Name",FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField PhysicalDeliveryOffice = new ActiveDirectoryField() { Id = 3, FieldName = "physicalDeliveryOfficeName", DisplayName ="Office", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField EmployeeId = new ActiveDirectoryField() { Id = 4, FieldName = "employeeId", DisplayName ="Employee ID", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField HomeDirectory = new ActiveDirectoryField() { Id = 5, FieldName = "homeDirectory", DisplayName ="Home Directory", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField ScriptPath = new ActiveDirectoryField() { Id = 6, FieldName = "scriptPath", DisplayName ="Logon Script Path", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField ProfilePath = new ActiveDirectoryField() { Id = 7, FieldName = "profilePath", DisplayName ="Profile Path", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField HomePhone = new ActiveDirectoryField() { Id = 8, FieldName = "homePhone", DisplayName ="Home Phone Number", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField StreetAddress = new ActiveDirectoryField() { Id = 9, FieldName = "streetAddress", DisplayName ="Street Address", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField City = new ActiveDirectoryField() { Id = 10, FieldName = "l", DisplayName ="City", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField State = new ActiveDirectoryField() { Id = 11, FieldName = "st", DisplayName ="State", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField PostalCode = new ActiveDirectoryField() { Id = 12, FieldName = "postalCode", DisplayName ="Zip Code", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField Site = new ActiveDirectoryField() { Id = 13, FieldName = "site", DisplayName ="Site", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField Name = new ActiveDirectoryField() { Id = 14, FieldName = "name", DisplayName ="Name", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField SAMAccountName = new ActiveDirectoryField() { Id = 15, FieldName = "samaccountname", DisplayName ="Username", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField ObjectSID = new ActiveDirectoryField() { Id = 16, FieldName = "objectSID", DisplayName ="SID", FieldType = ActiveDirectoryFieldType.RawData };
        public static ActiveDirectoryField Mail = new ActiveDirectoryField() { Id = 17, FieldName = "mail", DisplayName ="E-Mail Address", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField Description = new ActiveDirectoryField() { Id = 18, FieldName = "description", DisplayName ="Description", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField DisplayName = new ActiveDirectoryField() { Id = 19, FieldName = "displayName", DisplayName ="Display Name", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField DistinguishedName = new ActiveDirectoryField() { Id = 20, FieldName = "distinguishedName", DisplayName ="Distinguished Name", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField MemberOf = new ActiveDirectoryField() { Id = 21, FieldName = "memberOf", DisplayName ="Member Of", FieldType = ActiveDirectoryFieldType.List };
        public static ActiveDirectoryField Company = new ActiveDirectoryField() { Id = 22, FieldName = "company", DisplayName ="Company", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField Title = new ActiveDirectoryField() { Id = 23, FieldName = "title", DisplayName ="Title", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField UserPrincipalName = new ActiveDirectoryField() { Id = 24, FieldName = "userPrincipalName", DisplayName ="User Principal Name", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField TelephoneNumber = new ActiveDirectoryField() { Id = 25, FieldName = "telephoneNumber", DisplayName ="Telephone Number", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField POBox = new ActiveDirectoryField() { Id = 26, FieldName = "postOfficeBox", DisplayName ="PO Box", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField CanonicalName = new ActiveDirectoryField() { Id = 27, FieldName = "cn", DisplayName = "Canonical Name", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField HomeDrive = new ActiveDirectoryField() { Id = 28, FieldName = "homeDrive", DisplayName ="Home Drive", FieldType = ActiveDirectoryFieldType.DriveLetter };
        public static ActiveDirectoryField Department = new ActiveDirectoryField() { Id = 29, FieldName = "department",DisplayName="Department", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField MiddleName = new ActiveDirectoryField() { Id = 30, FieldName = "middleName",DisplayName="Middle Name", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField Pager = new ActiveDirectoryField() { Id = 31, FieldName = "pager",DisplayName="Pager", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField OperatingSystem = new ActiveDirectoryField() { Id = 32, FieldName = "operatingSystemVersion", DisplayName="OS", FieldType = ActiveDirectoryFieldType.Text };
        public static ActiveDirectoryField AccountExpires = new ActiveDirectoryField() { Id = 33, FieldName = "accountExpires", DisplayName="Account Expiration", FieldType = ActiveDirectoryFieldType.Date };
        public static ActiveDirectoryField Manager = new ActiveDirectoryField() { Id = 34, FieldName = "manager", DisplayName="Manager", FieldType = ActiveDirectoryFieldType.Text };
#pragma warning restore CA2211 // Non-constant fields should not be visible


    }
}
