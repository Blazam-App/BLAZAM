
using BLAZAM.Common.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;

namespace BLAZAM.Database.Models
{
    public enum ActiveDirectoryFieldType
    {
        Text, Date, RawData,
        DriveLetter,
        List
    }
    /// <summary>
    /// Represents a built-in standard Active Directory attribute
    /// </summary>
    public class ActiveDirectoryField : AppDbSetBase, IActiveDirectoryField
    {

        
        [Required]
        public string FieldName { get; set; }


        
        [Required]
        public string DisplayName { get; set; }


        
        public ActiveDirectoryFieldType FieldType { get; set; } = ActiveDirectoryFieldType.Text;




        
        public override string? ToString()
        {
            return DisplayName;
        }

        
        public override int GetHashCode()
        {
            if (FieldName == null) return Id.GetHashCode();
            return FieldName.GetHashCode();
        }

        
        public override bool Equals(object? obj)
        {
            if (obj is ActiveDirectoryField)
            {
                var other = obj as ActiveDirectoryField;

                if (other?.FieldName == FieldName)
                {
                    return true;
                }

            }
            return false;
        }
        
        public bool IsActionAppropriateForObject(ActiveDirectoryObjectType objectType)
        {

            switch (objectType)
            {
                case ActiveDirectoryObjectType.User:
                    switch (FieldName)
                    {
                        case "l":
                        case "cn":
                        case "company":
                        case "depatment":
                        case "description":
                        case "displayName":
                        case "distinguishedName":
                        case "employeeId":
                        case "givenname":
                        case "homeDirectory":
                        case "homeDrive":
                        case "homePhone":
                        case "manager":
                        case "mail":
                        case "memberOf":
                        case "middleName":
                        case "objectSID":
                        case "pager":
                        case "physicalDeliveryOffice":
                        case "postalCode":
                        case "profilePath":
                        case "samaccountname":
                        case "scriptPath":
                        case "site":
                        case "sn":
                        case "st":
                        case "street":
                        case "streetAddress":
                        case "telephoneNumber":
                        case "title":
                        case "thumbnail":
                        case "userPrincipalName":
                            return true;
                    }
                    break;
                case ActiveDirectoryObjectType.Computer:
                    switch (FieldName)
                    {
                        case "cn":
                        case "description":
                        case "displayName":
                        case "distinguishedName":
                        case "memberOf":
                        case "objectSID":
                        case "operatingSystemVersion":
                        case "samaccountname":
                        case "site":
                            return true;
                    }
                    break;

                case ActiveDirectoryObjectType.Group:
                    switch (FieldName)
                    {
                        case "cn":
                        case "description":
                        case "displayName":
                        case "distinguishedName":
                        case "mail":
                        case "memberOf":
                        case "objectSID":
                        case "samaccountname":
                        case "site":
                            return true;
                    }
                    break;

                case ActiveDirectoryObjectType.OU:
                    switch (FieldName)
                    {
                        case "cn":
                        case "description":
                        case "displayName":
                        case "distinguishedName":
                        case "objectSID":
                        case "site":
                            return true;


                    }
                    break;


            }
            return false;


        }


    }

}
