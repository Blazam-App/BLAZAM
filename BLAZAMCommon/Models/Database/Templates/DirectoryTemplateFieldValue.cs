namespace BLAZAM.Common.Models.Database.Templates
{
    public class DirectoryTemplateFieldValue : AppDbSetBase, ICloneable
    {
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