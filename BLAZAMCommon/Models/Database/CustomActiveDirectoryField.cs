using BLAZAM.Common.Data.ActiveDirectory;
using BLAZAM.Common.Models.Database.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;

namespace BLAZAM.Common.Models.Database
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

                if (other.FieldName == this.FieldName)
                {
                    return true;
                }

            }
            return false;
        }


        /// <inheritdoc/>
        public bool IsActionAppropriateForObject(ActiveDirectoryObjectType objectType)
        {
            return this.ObjectTypes.Any(ot=>ot.ObjectType == objectType);
           

        }


    }

}
