using BLAZAM.Common.Models.Database;
using System.Reflection;

namespace BLAZAM.Common.Models.Database
{
    
    public class ActiveDirectoryFields
    {



        public static List<ActiveDirectoryField> Fields = new List<ActiveDirectoryField>() {
            SN,
            GivenName,
            PhysicalDeliveryOffice,
            EmployeeId,
            HomeDirectory,
            ScriptPath,
            ProfilePath,
            HomePhone,
            StreetAddress,
            City,
            State,
            PostalCode,
            Site,
            Name,
            SAMAccountName,
            ObjectSID,
            Mail,
            Description,
            DisplayName,
            DistinguishedName,
            MemberOf,
            Pager,
            OperatingSystem,
            CanonicalName,
            AccountExpires

        };
        public static ActiveDirectoryField SN = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1, FieldName = "sn", DisplayName ="Last Name"};
        public static ActiveDirectoryField GivenName = new ActiveDirectoryField() { ActiveDirectoryFieldId = 2, FieldName = "givenname", DisplayName ="First Name" };
        public static ActiveDirectoryField PhysicalDeliveryOffice = new ActiveDirectoryField() { ActiveDirectoryFieldId = 3, FieldName = "physicalDeliveryOfficeName", DisplayName ="Office"};
        public static ActiveDirectoryField EmployeeId = new ActiveDirectoryField() { ActiveDirectoryFieldId = 4, FieldName = "employeeId", DisplayName ="Employee ID"};
        public static ActiveDirectoryField HomeDirectory = new ActiveDirectoryField() { ActiveDirectoryFieldId = 5, FieldName = "homeDirectory", DisplayName ="Home Directory"};
        public static ActiveDirectoryField ScriptPath = new ActiveDirectoryField() { ActiveDirectoryFieldId = 6, FieldName = "scriptPath", DisplayName ="Logon Script Path"};
        public static ActiveDirectoryField ProfilePath = new ActiveDirectoryField() { ActiveDirectoryFieldId = 7, FieldName = "profilePath", DisplayName ="Profile Path"};
        public static ActiveDirectoryField HomePhone = new ActiveDirectoryField() { ActiveDirectoryFieldId = 8, FieldName = "homePhone", DisplayName ="Home Phone Number" };
        public static ActiveDirectoryField StreetAddress = new ActiveDirectoryField() { ActiveDirectoryFieldId = 9, FieldName = "streetAddress", DisplayName ="Street Address"};
        public static ActiveDirectoryField City = new ActiveDirectoryField() { ActiveDirectoryFieldId = 10, FieldName = "city", DisplayName ="City"};
        public static ActiveDirectoryField State = new ActiveDirectoryField() { ActiveDirectoryFieldId = 11, FieldName = "st", DisplayName ="State"};
        public static ActiveDirectoryField PostalCode = new ActiveDirectoryField() { ActiveDirectoryFieldId = 12, FieldName = "postalCode", DisplayName ="Zip Code"};
        public static ActiveDirectoryField Site = new ActiveDirectoryField() { ActiveDirectoryFieldId = 13, FieldName = "site", DisplayName ="Site" };
        public static ActiveDirectoryField Name = new ActiveDirectoryField() { ActiveDirectoryFieldId = 14, FieldName = "name", DisplayName ="Name"};
        public static ActiveDirectoryField SAMAccountName = new ActiveDirectoryField() { ActiveDirectoryFieldId = 15, FieldName = "samaccountname", DisplayName ="Username"};
        public static ActiveDirectoryField ObjectSID = new ActiveDirectoryField() { ActiveDirectoryFieldId = 16, FieldName = "objectSID", DisplayName ="SID"};
        public static ActiveDirectoryField Mail = new ActiveDirectoryField() { ActiveDirectoryFieldId = 17, FieldName = "mail", DisplayName ="E-Mail Address"};
        public static ActiveDirectoryField Description = new ActiveDirectoryField() { ActiveDirectoryFieldId = 18, FieldName = "description", DisplayName ="Description"};
        public static ActiveDirectoryField DisplayName = new ActiveDirectoryField() { ActiveDirectoryFieldId = 19, FieldName = "displayName", DisplayName ="Display Name"};
        public static ActiveDirectoryField DistinguishedName = new ActiveDirectoryField() { ActiveDirectoryFieldId = 20, FieldName = "distinguishedName", DisplayName ="Distinguished Name"};
        public static ActiveDirectoryField MemberOf = new ActiveDirectoryField() { ActiveDirectoryFieldId = 21, FieldName = "memberOf", DisplayName ="Member Of"};
        public static ActiveDirectoryField Company = new ActiveDirectoryField() { ActiveDirectoryFieldId = 22, FieldName = "company", DisplayName ="Company"};
        public static ActiveDirectoryField Title = new ActiveDirectoryField() { ActiveDirectoryFieldId = 23, FieldName = "title", DisplayName ="Title"};
        public static ActiveDirectoryField UserPrincipalName = new ActiveDirectoryField() { ActiveDirectoryFieldId = 24, FieldName = "userPrincipalName", DisplayName ="User Principal Name" };
        public static ActiveDirectoryField TelephoneNumber = new ActiveDirectoryField() { ActiveDirectoryFieldId = 25, FieldName = "telephoneNumber", DisplayName ="Telephone Number" };
        public static ActiveDirectoryField Street = new ActiveDirectoryField() { ActiveDirectoryFieldId = 26, FieldName = "street", DisplayName ="Street"};
        public static ActiveDirectoryField CanonicalName = new ActiveDirectoryField() { ActiveDirectoryFieldId = 27, FieldName = "cn", DisplayName = "Canonical Name" };
        public static ActiveDirectoryField HomeDrive = new ActiveDirectoryField() { ActiveDirectoryFieldId = 28, FieldName = "homeDrive", DisplayName ="Home Drive" };
        public static ActiveDirectoryField Department = new ActiveDirectoryField() { ActiveDirectoryFieldId = 29, FieldName = "department",DisplayName="Department" };
        public static ActiveDirectoryField MiddleName = new ActiveDirectoryField() { ActiveDirectoryFieldId = 30, FieldName = "middleName",DisplayName="Middle Name" };
        public static ActiveDirectoryField Pager = new ActiveDirectoryField() { ActiveDirectoryFieldId = 31, FieldName = "pager",DisplayName="Pager" };
        public static ActiveDirectoryField OperatingSystem = new ActiveDirectoryField() { ActiveDirectoryFieldId = 32, FieldName = "operatingSystemVersion", DisplayName="OS" };
        public static ActiveDirectoryField AccountExpires = new ActiveDirectoryField() { ActiveDirectoryFieldId = 33, FieldName = "accountExpires", DisplayName="Account Expiration" };

        
    }
}
