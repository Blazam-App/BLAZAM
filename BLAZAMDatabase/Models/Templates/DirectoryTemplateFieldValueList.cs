namespace BLAZAM.Database.Models.Templates
{
    public class DirectoryTemplateFieldValueList : AppDbSetBase
    {
        public string Name { get; set; }

        public List<DirectoryTemplateFieldValueListItem> Items { get; set; } = new List<DirectoryTemplateFieldValueListItem>();
    }
}
