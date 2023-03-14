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
        public static ActiveDirectoryField SN = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000001, FieldName = "sn", DisplayName ="Last Name"};
        public static ActiveDirectoryField GivenName = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000002, FieldName = "givenname", DisplayName ="First Name" };
        public static ActiveDirectoryField PhysicalDeliveryOffice = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000003, FieldName = "physicalDeliveryOfficeName", DisplayName ="Office"};
        public static ActiveDirectoryField EmployeeId = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000004, FieldName = "employeeId", DisplayName ="Employee ID"};
        public static ActiveDirectoryField HomeDirectory = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000005, FieldName = "homeDirectory", DisplayName ="Home Directory"};
        public static ActiveDirectoryField ScriptPath = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000006, FieldName = "scriptPath", DisplayName ="Logon Script Path"};
        public static ActiveDirectoryField ProfilePath = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000007, FieldName = "profilePath", DisplayName ="Profile Path"};
        public static ActiveDirectoryField HomePhone = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000008, FieldName = "homePhone", DisplayName ="Home Phone Number" };
        public static ActiveDirectoryField StreetAddress = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000009, FieldName = "streetAddress", DisplayName ="Street Address"};
        public static ActiveDirectoryField City = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000010, FieldName = "city", DisplayName ="City"};
        public static ActiveDirectoryField State = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000011, FieldName = "st", DisplayName ="State"};
        public static ActiveDirectoryField PostalCode = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000012, FieldName = "postalCode", DisplayName ="Zip Code"};
        public static ActiveDirectoryField Site = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000013, FieldName = "site", DisplayName ="Site" };
        public static ActiveDirectoryField Name = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000014, FieldName = "name", DisplayName ="Name"};
        public static ActiveDirectoryField SAMAccountName = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000015, FieldName = "samaccountname", DisplayName ="Username"};
        public static ActiveDirectoryField ObjectSID = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000016, FieldName = "objectSID", DisplayName ="SID"};
        public static ActiveDirectoryField Mail = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000017, FieldName = "mail", DisplayName ="E-Mail Address"};
        public static ActiveDirectoryField Description = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000018, FieldName = "description", DisplayName ="Description"};
        public static ActiveDirectoryField DisplayName = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000019, FieldName = "displayName", DisplayName ="Display Name"};
        public static ActiveDirectoryField DistinguishedName = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000020, FieldName = "distinguishedName", DisplayName ="Distinguished Name"};
        public static ActiveDirectoryField MemberOf = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000021, FieldName = "memberOf", DisplayName ="Member Of"};
        public static ActiveDirectoryField Company = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000022, FieldName = "company", DisplayName ="Company"};
        public static ActiveDirectoryField Title = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000023, FieldName = "title", DisplayName ="Title"};
        public static ActiveDirectoryField UserPrincipalName = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000024, FieldName = "userPrincipalName", DisplayName ="User Principal Name" };
        public static ActiveDirectoryField TelephoneNumber = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000025, FieldName = "telephoneNumber", DisplayName ="Telephone Number" };
        public static ActiveDirectoryField Street = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000026, FieldName = "street", DisplayName ="Street"};
        public static ActiveDirectoryField CanonicalName = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000027, FieldName = "cn", DisplayName = "Canonical Name" };
        public static ActiveDirectoryField HomeDrive = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000028, FieldName = "homeDrive", DisplayName ="Home Drive" };
        public static ActiveDirectoryField Department = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000029, FieldName = "department",DisplayName="Department" };
        public static ActiveDirectoryField MiddleName = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000030, FieldName = "middleName",DisplayName="Middle Name" };
        public static ActiveDirectoryField Pager = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000031, FieldName = "pager",DisplayName="Pager" };
        public static ActiveDirectoryField OperatingSystem = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000032, FieldName = "operatingSystemVersion", DisplayName="OS" };
        public static ActiveDirectoryField AccountExpires = new ActiveDirectoryField() { ActiveDirectoryFieldId = 1000033, FieldName = "accountExpires", DisplayName="Account Expiration" };
#pragma warning restore CA2211 // Non-constant fields should not be visible


    }
}
