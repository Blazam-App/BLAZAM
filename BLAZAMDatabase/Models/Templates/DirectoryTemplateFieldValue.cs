using BLAZAM.Database.Context;
using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Database.Models.Templates
{
    public class DirectoryTemplateFieldValue : AppDbSetBase
    {
        public ActiveDirectoryField? Field { get; set; }
        public CustomActiveDirectoryField? CustomField { get; set; }
        public string? Value { get; set; } = "";

        /// <summary>
        /// Indicates whether a regular user should be able to modify
        /// this field's value
        /// </summary>
        public bool Editable { get; set; }

        /// <summary>
        /// Require the field to have a value
        /// </summary>
        public bool Required { get; set; }

        [NotMapped]
        public string FieldDisplayName => Field != null ? Field?.DisplayName : CustomField?.DisplayName;

        public object Clone(IDatabaseContext context)
        {
            var clone = new DirectoryTemplateFieldValue()
            {

                Field = context.ActiveDirectoryFields.FirstOrDefault(f => f.Id == Field.Id),
                CustomField = CustomField,
                Value = Value,
                Editable = Editable,
                Required = Required
            };
            return clone;
        }

        public override string? ToString()
        {
            if (Field != null) return Field.ToString() + "=" + Value;
            return CustomField?.ToString() + "=" + Value;
        }
        public override bool Equals(object? obj)
        {
            if (!base.Equals(obj)) return false;
            if (obj is DirectoryTemplateFieldValue other)
            {
                return other.FieldDisplayName == FieldDisplayName && other.Value == Value;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return FieldDisplayName.GetHashCode();
        }
    }
}