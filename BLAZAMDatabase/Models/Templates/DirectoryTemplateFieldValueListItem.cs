using System.ComponentModel.DataAnnotations;

namespace BLAZAM.Database.Models.Templates
{
    public class DirectoryTemplateFieldValueListItem : AppDbSetBase
    {
        [Required]
        public string StringValue { get; set; }
    }
}
