
using BLAZAM.Common.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Database.Models
{
    public enum ActiveDirectoryFieldOperator
    {
        EqualTo,
        Contains,
        StartsWith,
        EndsWith,
        HistoricalTimeFrame,
        FutureTimeFrame,
        BeforeNow,
        AfterNow,
        Boolean
    }
    public enum ActiveDirectoryFieldType
    {
        Text, Date, RawData,
        DriveLetter,
        StringList, FileTime,
        Boolean
    }
    /// <summary>
    /// Represents a built-in standard Active Directory attribute
    /// </summary>
    public class ActiveDirectoryField : RecoverableAppDbSetBase, IActiveDirectoryField
    {
        private const string _desription = "description";
        private const string _memberOf = "memberOf";

        [NotMapped]
        public new DateTime? DeletedAt { get => null; set { _ = value; } }

        [Required]
        public string FieldName { get; set; }



        [Required]
        public string DisplayName { get; set; }


        /// <summary>
        /// The name of the reflection property in the IDirectoryEntryAdapter
        /// </summary>
        public string PropertyName { get; set; }



        public ActiveDirectoryFieldType FieldType { get; set; } = ActiveDirectoryFieldType.Text;





        public override string? ToString()
        {
            return DisplayName;
        }


        public override int GetHashCode()
        {
            if (FieldName == null)
            {
                return Id.GetHashCode();
            }

            return FieldName.GetHashCode();
        }


        public override bool Equals(object? obj)
        {
            if (obj is ActiveDirectoryField other && other.FieldName == FieldName)
            {
                return true;
            }

            return false;
        }

        public bool IsFieldAppropriateForObject(ActiveDirectoryObjectType objectType)
        {

            switch (objectType)
            {
                case ActiveDirectoryObjectType.User:
                    switch (FieldName)
                    {
                        case "l":
                        case "company":
                        case "department":
                        case _desription:
                        case "employeeId":
                        case "givenname":
                        case "homeDirectory":
                        case "homeDrive":
                        case "homePhone":
                        case "logonHours":
                        case "manager":
                        case "mail":
                        case _memberOf:
                        case "middleName":
                        case "pager":
                        case "physicalDeliveryOffice":
                        case "postalCode":
                        case "profilePath":
                        case "scriptPath":
                        case "sn":
                        case "st":
                        case "streetAddress":
                        case "telephoneNumber":
                        case "title":
                        case "thumbnail":
                        case "userWorkstations":

                            return true;
                    }

                    break;
                case ActiveDirectoryObjectType.Contact:
                    switch (FieldName)
                    {
                        case "l":
                        case "cn":
                        case "company":
                        case "department":
                        case _desription:
                        case "displayName":
                        case "distinguishedName":
                        case "employeeId":
                        case "givenname":
                        case "homePhone":
                        case "manager":
                        case "mail":
                        case _memberOf:
                        case "middleName":
                        case "objectSID":
                        case "pager":
                        case "physicalDeliveryOffice":
                        case "postalCode":
                        case "sn":
                        case "st":
                        case "streetAddress":
                        case "telephoneNumber":
                        case "title":
                        case "thumbnail":

                            return true;
                    }

                    break;
                case ActiveDirectoryObjectType.Computer:
                    switch (FieldName)
                    {
                        case _memberOf:
                        case _desription:
                        case "operatingSystemVersion":
                        case "msLAPS-Password":
                            return true;
                    }

                    break;

                case ActiveDirectoryObjectType.Group:
                    switch (FieldName)
                    {
                        case _desription:
                        case "mail":
                        case _memberOf:
                        case "groupType":
                            return true;
                    }

                    break;

                case ActiveDirectoryObjectType.OU:

                    break;
                default:
                    switch (FieldName)
                    {
                        case "cn":
                        case _desription:
                        case "displayName":
                        case "distinguishedName":
                        case "objectSID":
                            return true;
                    }

                    break;

            }

            return false;
        }

    }


}
