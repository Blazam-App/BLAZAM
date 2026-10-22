using BLAZAM.Localization;
using System.ComponentModel.DataAnnotations;

namespace BLAZAM.Database.Models
{
    public class CustomStringList : RecoverableAppDbSetBase
    {
        [Required(ErrorMessageResourceName = "Field_Required", ErrorMessageResourceType = typeof(AppValidationLocalization))]
        public string Name { get; set; }

        public List<CustomStringListItem> Values { get; set; } = [];

        public List<CustomStringListField> AssignedFields { get; set; } = [];

        public override string? ToString()
        {
            return Name;
        }
    }
}
