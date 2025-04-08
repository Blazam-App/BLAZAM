using BLAZAM.ActiveDirectory.Data;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Exceptions;
using BLAZAM.Database.Models;
using BLAZAM.FileSystem;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Security.AccessControl;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class ADContact : AccountDirectoryAdapter, IADContact
    {
        public byte[]? ThumbnailPhoto
        {

            get
            {
                return GetProperty<byte[]>("thumbnailPhoto");
            }
            set
            {
                SetProperty("thumbnailPhoto", value);
            }
        }
        public string? Pager
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.Pager.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.Pager.FieldName, value);
            }
        }
        public string? GivenName
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.GivenName.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.GivenName.FieldName, value);
            }
        }
      
        public string? MiddleName
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.MiddleName.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.MiddleName.FieldName, value);
            }
        }
        public string? Surname
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.SN.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.SN.FieldName, value);
            }
        }

        [Required]
        public override string? DisplayName { get => base.DisplayName; set => base.DisplayName = value; }

        public string? Department
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.Department.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.Department.FieldName, value);
            }
        }
        public string? PhysicalDeliveryOfficeName
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.PhysicalDeliveryOffice.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.PhysicalDeliveryOffice.FieldName, value);
            }
        }
        public string? EmployeeId
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.EmployeeId.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.EmployeeId.FieldName, value);
            }
        }
        

     
       

      
     

        public string? Title
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.Title.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.Title.FieldName, value);
            }
        }

        public string? Company
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.Company.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.Company.FieldName, value);
            }
        }

        public string? TelephoneNumber
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.TelephoneNumber.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.TelephoneNumber.FieldName, value);
            }

        }

        public string? HomePhone
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.HomePhone.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.HomePhone.FieldName, value);
            }

        }
        public string? StreetAddress
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.StreetAddress.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.StreetAddress.FieldName, value);
            }
        }
        public string? POBox
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.POBox.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.POBox.FieldName, value);
            }
        }
        public string? City
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.City.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.City.FieldName, value);
            }
        }
        public string? State
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.State.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.State.FieldName, value);
            }
        }
        public string? Zip
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.PostalCode.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.PostalCode.FieldName, value);
            }
        }
        public string? Site
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.Site.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.Site.FieldName, value);
            }
        }



    }
}
