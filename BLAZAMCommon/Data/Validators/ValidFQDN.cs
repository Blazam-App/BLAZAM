using BLAZAM.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace BLAZAM.Common.Data.Validators
{
    public class ValidFQDN : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value != null && value is string strValue)
            {
                if (!strValue.IsNullOrEmpty())
                    if (!Regex.IsMatch(strValue, "^[A-Z0-9.-]+\\.[A-Z]{2,}$", RegexOptions.IgnoreCase))
                        return new ValidationResult("Must be a valid domain name.");

            }
            return ValidationResult.Success;


        }
    }
}
