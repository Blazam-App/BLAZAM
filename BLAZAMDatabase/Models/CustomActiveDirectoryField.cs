using System.ComponentModel.DataAnnotations;
using BLAZAM.Common.Data;
using BLAZAM.Localization;

namespace BLAZAM.Database.Models
{
    public class CustomActiveDirectoryField : RecoverableAppDbSetBase, IActiveDirectoryField
    {


        [Required(ErrorMessageResourceName = "CustomActiveDirectoryField_FieldName", ErrorMessageResourceType = typeof(AppValidationLocalization))]
        public string FieldName { get; set; }


        [Required(ErrorMessageResourceName = "CustomActiveDirectoryField_DisplayName", ErrorMessageResourceType = typeof(AppValidationLocalization))]
        public string DisplayName { get; set; }



        public ActiveDirectoryFieldType FieldType { get; set; } = ActiveDirectoryFieldType.Text;


        [Required(ErrorMessageResourceName = "CustomActiveDirectoryField_ObjectTypes", ErrorMessageResourceType = typeof(AppValidationLocalization))]
        public List<ActiveDirectoryFieldObjectType> ObjectTypes { get; set; } = [];



        public override string? ToString()
        {
            return DisplayName;
        }


        public override int GetHashCode()
        {
            if (DisplayName == null)
            {
                return Id.GetHashCode();
            }

            return DisplayName.GetHashCode();
        }


        public override bool Equals(object? obj)
        {
            if (obj is CustomActiveDirectoryField other && other.Id == Id)
            {
                return true;
            }

            return false;
        }



        public bool IsFieldAppropriateForObject(ActiveDirectoryObjectType objectType)
        {
            return ObjectTypes.Any(ot => ot.ObjectType == objectType);


        }


    }

}
