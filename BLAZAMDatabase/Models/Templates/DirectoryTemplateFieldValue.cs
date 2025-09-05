using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Database.Models.Templates
{
    public class DirectoryTemplateFieldValue : AppDbSetBase
    {
        public ActiveDirectoryField? Field { get; set; }
        public int? FieldId { get; set; }
        public CustomActiveDirectoryField? CustomField { get; set; }
        public int? CustomFieldId { get; set; }

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
        public string? FieldName => Field != null ? Field?.FieldName : CustomField?.FieldName;

        [NotMapped]
        public string? FieldDisplayName => Field != null ? Field?.DisplayName : CustomField?.DisplayName;

        public object Clone(IDatabaseContext context)
        {
            var clone = new DirectoryTemplateFieldValue()
            {

                Field = Field != null ? context.ActiveDirectoryFields.FirstOrDefault(f => f.Id == Field.Id) : null,
                CustomField = CustomField != null ? context.CustomActiveDirectoryFields.FirstOrDefault(f => f.Id == CustomField.Id) : null,
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
        [NotMapped]
        public DateTime? DeletedAt
        {
            get
            {
                if (Field != null)
                {
                    return Field.DeletedAt;
                }
                else if (CustomField != null)
                {
                    return CustomField.DeletedAt;
                }
                return null;
            }
            set
            {
                if (Field != null)
                {
                    Field.DeletedAt = value;
                }

                else if (CustomField != null)
                {
                    CustomField.DeletedAt = value;
                }
            }
        }

    }
}