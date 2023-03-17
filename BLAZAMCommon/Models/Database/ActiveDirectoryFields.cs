using BLAZAM.Common.Models.Database;
using System.Reflection;

namespace BLAZAM.Common.Models.Database
{
    
    public class ActiveDirectoryFields
    {



        public static List<ActiveDirectoryField> Fields = new List<ActiveDirectoryField>() {
#pragma warning disable CS8604 // Possible null reference argument.
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
#pragma warning restore CS8604 // Possible null reference argument.

        };
#pragma warning disable CA2211 // Non-constant fields should not be visible
        public static ActiveDirectoryField SN = new ActiveDirectoryField() { Id = 1, FieldName = "sn", DisplayName ="Last Name"};
        public static ActiveDirectoryField GivenName = new ActiveDirectoryField() { Id = 2, FieldName = "givenname", DisplayName ="First Name" };
        public static ActiveDirectoryField PhysicalDeliveryOffice = new ActiveDirectoryField() { Id = 3, FieldName = "physicalDeliveryOfficeName", DisplayName ="Office"};
        public static ActiveDirectoryField EmployeeId = new ActiveDirectoryField() { Id = 4, FieldName = "employeeId", DisplayName ="Employee ID"};
        public static ActiveDirectoryField HomeDirectory = new ActiveDirectoryField() { Id = 5, FieldName = "homeDirectory", DisplayName ="Home Directory"};
        public static ActiveDirectoryField ScriptPath = new ActiveDirectoryField() { Id = 6, FieldName = "scriptPath", DisplayName ="Logon Script Path"};
        public static ActiveDirectoryField ProfilePath = new ActiveDirectoryField() { Id = 7, FieldName = "profilePath", DisplayName ="Profile Path"};
        public static ActiveDirectoryField HomePhone = new ActiveDirectoryField() { Id = 8, FieldName = "homePhone", DisplayName ="Home Phone Number" };
        public static ActiveDirectoryField StreetAddress = new ActiveDirectoryField() { Id = 9, FieldName = "streetAddress", DisplayName ="Street Address"};
        public static ActiveDirectoryField City = new ActiveDirectoryField() { Id = 10, FieldName = "city", DisplayName ="City"};
        public static ActiveDirectoryField State = new ActiveDirectoryField() { Id = 11, FieldName = "st", DisplayName ="State"};
        public static ActiveDirectoryField PostalCode = new ActiveDirectoryField() { Id = 12, FieldName = "postalCode", DisplayName ="Zip Code"};
        public static ActiveDirectoryField Site = new ActiveDirectoryField() { Id = 13, FieldName = "site", DisplayName ="Site" };
        public static ActiveDirectoryField Name = new ActiveDirectoryField() { Id = 14, FieldName = "name", DisplayName ="Name"};
        public static ActiveDirectoryField SAMAccountName = new ActiveDirectoryField() { Id = 15, FieldName = "samaccountname", DisplayName ="Username"};
        public static ActiveDirectoryField ObjectSID = new ActiveDirectoryField() { Id = 16, FieldName = "objectSID", DisplayName ="SID"};
        public static ActiveDirectoryField Mail = new ActiveDirectoryField() { Id = 17, FieldName = "mail", DisplayName ="E-Mail Address"};
        public static ActiveDirectoryField Description = new ActiveDirectoryField() { Id = 18, FieldName = "description", DisplayName ="Description"};
        public static ActiveDirectoryField DisplayName = new ActiveDirectoryField() { Id = 19, FieldName = "displayName", DisplayName ="Display Name"};
        public static ActiveDirectoryField DistinguishedName = new ActiveDirectoryField() { Id = 20, FieldName = "distinguishedName", DisplayName ="Distinguished Name"};
        public static ActiveDirectoryField MemberOf = new ActiveDirectoryField() { Id = 21, FieldName = "memberOf", DisplayName ="Member Of"};
        public static ActiveDirectoryField Company = new ActiveDirectoryField() { Id = 22, FieldName = "company", DisplayName ="Company"};
        public static ActiveDirectoryField Title = new ActiveDirectoryField() { Id = 23, FieldName = "title", DisplayName ="Title"};
        public static ActiveDirectoryField UserPrincipalName = new ActiveDirectoryField() { Id = 24, FieldName = "userPrincipalName", DisplayName ="User Principal Name" };
        public static ActiveDirectoryField TelephoneNumber = new ActiveDirectoryField() { Id = 25, FieldName = "telephoneNumber", DisplayName ="Telephone Number" };
        public static ActiveDirectoryField Street = new ActiveDirectoryField() { Id = 26, FieldName = "street", DisplayName ="Street"};
        public static ActiveDirectoryField CanonicalName = new ActiveDirectoryField() { Id = 27, FieldName = "cn", DisplayName = "Canonical Name" };
        public static ActiveDirectoryField HomeDrive = new ActiveDirectoryField() { Id = 28, FieldName = "homeDrive", DisplayName ="Home Drive" };
        public static ActiveDirectoryField Department = new ActiveDirectoryField() { Id = 29, FieldName = "department",DisplayName="Department" };
        public static ActiveDirectoryField MiddleName = new ActiveDirectoryField() { Id = 30, FieldName = "middleName",DisplayName="Middle Name" };
        public static ActiveDirectoryField Pager = new ActiveDirectoryField() { Id = 31, FieldName = "pager",DisplayName="Pager" };
        public static ActiveDirectoryField OperatingSystem = new ActiveDirectoryField() { Id = 32, FieldName = "operatingSystemVersion", DisplayName="OS" };
        public static ActiveDirectoryField AccountExpires = new ActiveDirectoryField() { Id = 33, FieldName = "accountExpires", DisplayName="Account Expiration" };
#pragma warning restore CA2211 // Non-constant fields should not be visible


    }
}
