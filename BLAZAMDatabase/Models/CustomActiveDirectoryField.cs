using BLAZAM.Common.Data;
using System.ComponentModel.DataAnnotations;

namespace BLAZAM.Database.Models
{
    public class CustomActiveDirectoryField : RecoverableAppDbSetBase, IActiveDirectoryField
    {

        /// <inheritdoc/>
        [Required]
        public string FieldName { get; set; }

        /// <inheritdoc/>
        [Required]
        public string DisplayName { get; set; }


        /// <inheritdoc/>
        public ActiveDirectoryFieldType FieldType { get; set; } = ActiveDirectoryFieldType.Text;


        /// <inheritdoc/>
        [Required]
        public List<ActiveDirectoryFieldObjectType> ObjectTypes { get; set; }


        /// <inheritdoc/>
        public override string? ToString()
        {
            return FieldName;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            if (FieldName == null) return Id.GetHashCode();
            return FieldName.GetHashCode();
        }

        /// <inheritdoc/>
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


        /// <inheritdoc/>
        public bool IsActionAppropriateForObject(ActiveDirectoryObjectType objectType)
        {
            return ObjectTypes.Any(ot => ot.ObjectType == objectType);


        }


    }

}
