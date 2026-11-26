using System.ComponentModel.DataAnnotations;

namespace BLAZAM.Database.Models.Templates
{
    public class DirectoryTemplateFieldValueList : AppDbSetBase
    {
        [Required]
        public string Name { get; set; }

        public List<DirectoryTemplateFieldValueListItem> Items { get; set; } = new List<DirectoryTemplateFieldValueListItem>();
    }
}
