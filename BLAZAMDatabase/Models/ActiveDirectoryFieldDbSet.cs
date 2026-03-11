using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Database.Models
{
    public class ActiveDirectoryFieldDbSet : AppDbSetBase
    {
        [NotMapped]
        public IActiveDirectoryField? CurrentField
        {
            get
            {
                if (Field != null)
                {
                    return Field;
                }

                if (CustomField != null)
                {
                    return CustomField;
                }

                return null;
            }
            set
            {
                if (value is CustomActiveDirectoryField field)
                {
                    Field = null;
                    CustomField = @field;
                    CustomFieldId = @field.Id;
                }
                else if (value is ActiveDirectoryField field2)
                {
                    CustomField = null;
                    Field = field2;
                    FieldId = field2.Id;

                }

            }
        }

        [NotMapped]
        public int? CurrentFieldId
        {
            get
            {
                if (FieldId != null)
                {
                    return FieldId;
                }

                if (CustomFieldId != null)
                {
                    return CustomFieldId;
                }

                return null;
            }

        }

        public ActiveDirectoryField? Field { get; set; }
        public int? FieldId { get; set; }
        public CustomActiveDirectoryField? CustomField { get; set; }
        public int? CustomFieldId { get; set; }
    }
}
