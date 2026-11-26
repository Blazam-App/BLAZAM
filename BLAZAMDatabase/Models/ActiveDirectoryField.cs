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
        /// <summary>
        /// Determines whether the current field is valid for the specified Active Directory object type.
        /// </summary>
        /// <remarks>Supports specific field sets for User, Contact, Computer, and Group. For unsupported
        /// or unhandled object types, only the description field is treated as valid.</remarks>
        /// <param name="objectType">The Active Directory object type to validate against.</param>
        /// <returns>true if the field is supported for the specified object type; otherwise, false.</returns>
        public bool IsFieldAppropriateForObject(ActiveDirectoryObjectType objectType)
        {
            switch (objectType)
            {
                case ActiveDirectoryObjectType.User:
                    switch (FieldName)
                    {
                        case ActiveDirectoryFields.CityFieldName:
                        case ActiveDirectoryFields.CompanyFieldName:
                        case ActiveDirectoryFields.DepartmentFieldName:
                        case ActiveDirectoryFields.DescriptionFieldName:
                        case ActiveDirectoryFields.EmployeeIdFieldName:
                        case ActiveDirectoryFields.HomeDirectoryFieldName:
                        case ActiveDirectoryFields.HomeDriveFieldName:
                        case ActiveDirectoryFields.HomePhoneFieldName:
                        case ActiveDirectoryFields.LogonHoursFieldName:
                        case ActiveDirectoryFields.ManagerFieldName:
                        case ActiveDirectoryFields.EmailFieldName:
                        case ActiveDirectoryFields.PagerFieldName:
                        case ActiveDirectoryFields.PhysicalDeliveryOfficeNameFieldName:
                        case ActiveDirectoryFields.ZipFieldName:
                        case ActiveDirectoryFields.ProfilePathFieldName:
                        case ActiveDirectoryFields.ScriptPathFieldName:
                        case ActiveDirectoryFields.StateFieldName:
                        case ActiveDirectoryFields.StreetAddressFieldName:
                        case ActiveDirectoryFields.TelephoneNumberFieldName:
                        case ActiveDirectoryFields.TitleFieldName:
                        case ActiveDirectoryFields.ThumbnailPhotoFieldName:
                        case ActiveDirectoryFields.LogOnToFieldName:
                            return true;
                    }

                    break;

                case ActiveDirectoryObjectType.Contact:
                    switch (FieldName)
                    {
                        case ActiveDirectoryFields.CityFieldName:
                        case ActiveDirectoryFields.CompanyFieldName:
                        case ActiveDirectoryFields.DepartmentFieldName:
                        case ActiveDirectoryFields.DescriptionFieldName:
                        case ActiveDirectoryFields.DisplayNameFieldName:
                        case ActiveDirectoryFields.DNFieldName:
                        case ActiveDirectoryFields.EmployeeIdFieldName:
                        case ActiveDirectoryFields.HomePhoneFieldName:
                        case ActiveDirectoryFields.ManagerFieldName:
                        case ActiveDirectoryFields.EmailFieldName:
                        case ActiveDirectoryFields.SIDFieldName:
                        case ActiveDirectoryFields.PagerFieldName:
                        case ActiveDirectoryFields.PhysicalDeliveryOfficeNameFieldName:
                        case ActiveDirectoryFields.ZipFieldName:
                        case ActiveDirectoryFields.StateFieldName:
                        case ActiveDirectoryFields.StreetAddressFieldName:
                        case ActiveDirectoryFields.TelephoneNumberFieldName:
                        case ActiveDirectoryFields.TitleFieldName:
                        case ActiveDirectoryFields.ThumbnailPhotoFieldName:
                            return true;
                    }

                    break;

                case ActiveDirectoryObjectType.Computer:
                    switch (FieldName)
                    {
                        case ActiveDirectoryFields.DescriptionFieldName:
                        case ActiveDirectoryFields.OSFieldName:
                        case ActiveDirectoryFields.LapsPasswordFieldName:
                            return true;
                    }

                    break;

                case ActiveDirectoryObjectType.Group:
                    switch (FieldName)
                    {
                        case ActiveDirectoryFields.DescriptionFieldName:
                        case ActiveDirectoryFields.EmailFieldName:
                        case ActiveDirectoryFields.MemberOfFieldName:
                        case ActiveDirectoryFields.GroupTypeFieldName:
                            return true;
                    }

                    break;

                case ActiveDirectoryObjectType.OU:
                    break;

                case ActiveDirectoryObjectType.BitLocker:
                    break;

                default:
                    switch (FieldName)
                    {
                        case ActiveDirectoryFields.DescriptionFieldName:
                            return true;
                    }

                    break;
            }

            return false;
        }

    }


}
