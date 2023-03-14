namespace BLAZAM.Database.Models.Database.Templates
{
    public class DirectoryTemplateFieldValue : ICloneable
    {
        public int DirectoryTemplateFieldValueId { get; set; }
        public ActiveDirectoryField Field { get; set; }
        public string Value { get; set; } = "";

        public object Clone()
        {
            var clone = new DirectoryTemplateFieldValue()
            {

                Field = Field,
                Value = Value
            };
            return clone;
        }

        public override string? ToString()
        {
            return Field.ToString() + "=" + Value;
        }
    }
}