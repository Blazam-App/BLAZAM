using System.ComponentModel.DataAnnotations;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Models;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class ADContact : GroupableDirectoryAdapter, IADContact
    {
        public byte[]? ThumbnailPhoto
        {

            get
            {
                return GetAttribute<byte[]>("thumbnailPhoto");
            }
            set
            {
                SetAttribute("thumbnailPhoto", value);
            }
        }
        public string? Pager
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.Pager.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.Pager.FieldName, value);
            }
        }
        public string? GivenName
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.GivenName.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.GivenName.FieldName, value);
            }
        }

        public string? MiddleName
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.MiddleName.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.MiddleName.FieldName, value);
            }
        }
        public string? Sn
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.SN.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.SN.FieldName, value);
            }
        }

        [Required]
        public override string? DisplayName { get => base.DisplayName; set => base.DisplayName = value; }

        public string? Department
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.Department.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.Department.FieldName, value);
            }
        }
        public string? PhysicalDeliveryOfficeName
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.PhysicalDeliveryOffice.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.PhysicalDeliveryOffice.FieldName, value);
            }
        }
        public string? EmployeeId
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.EmployeeId.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.EmployeeId.FieldName, value);
            }
        }








        public string? Title
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.Title.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.Title.FieldName, value);
            }
        }

        public string? Company
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.Company.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.Company.FieldName, value);
            }
        }

        public string? TelephoneNumber
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.TelephoneNumber.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.TelephoneNumber.FieldName, value);
            }

        }

        public string? HomePhone
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.HomePhone.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.HomePhone.FieldName, value);
            }

        }
        public string? StreetAddress
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.StreetAddress.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.StreetAddress.FieldName, value);
            }
        }
        public string? POBox
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.POBox.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.POBox.FieldName, value);
            }
        }
        public string? City
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.City.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.City.FieldName, value);
            }
        }
        public string? State
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.State.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.State.FieldName, value);
            }
        }
        public string? Zip
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.PostalCode.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.PostalCode.FieldName, value);
            }
        }
        public string? Site
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.Site.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.Site.FieldName, value);
            }
        }



    }
}
