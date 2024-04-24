using BLAZAM.Common.Data;
using System.ComponentModel.DataAnnotations;

namespace BLAZAM.Database.Models
{
    public class CustomActiveDirectoryField : RecoverableAppDbSetBase, IActiveDirectoryField
    {

        
        [Required]
        public string FieldName { get; set; }

        
        [Required]
        public string DisplayName { get; set; }


        
        public ActiveDirectoryFieldType FieldType { get; set; } = ActiveDirectoryFieldType.Text;


        
        [Required]
        public List<ActiveDirectoryFieldObjectType> ObjectTypes { get; set; }


        
        public override string? ToString()
        {
            return FieldName;
        }

        
        public override int GetHashCode()
        {
            if (FieldName == null) return Id.GetHashCode();
            return FieldName.GetHashCode();
        }

        
        public override bool Equals(object? obj)
        {
            if (obj is CustomActiveDirectoryField)
            {
                var other = obj as CustomActiveDirectoryField;

                if (other.FieldName == FieldName)
                {
                    return true;
                }

            }
            return false;
        }


        
        public bool IsActionAppropriateForObject(ActiveDirectoryObjectType objectType)
        {
            return ObjectTypes.Any(ot => ot.ObjectType == objectType);


        }


    }

}
