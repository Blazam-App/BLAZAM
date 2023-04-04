namespace BLAZAM.Common.Models.Database.Templates
{
    public class DirectoryTemplateFieldValue : AppDbSetBase, ICloneable
    {
        public ActiveDirectoryField Field { get; set; }
        public string Value { get; set; } = "";

        /// <summary>
        /// Indicates whether a regular user should be able to modify
        /// this field's value
        /// </summary>
        public bool Editable { get; set; }

        /// <summary>
        /// Require the field to have a value
        /// </summary>
        public bool Required { get; set; }


        public object Clone()
        {
            var clone = new DirectoryTemplateFieldValue()
            {

                Field = Field,
                Value = Value,
                Editable = Editable,
                Required = Required
            };
            return clone;
        }

        public override string? ToString()
        {
            return Field.ToString() + "=" + Value;
        }
    }
}