using BLAZAM.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace BLAZAM.Common.Data.Validators
{
    public class ValidFqdn : AppValidationAttribute
    {
        public ValidFqdn()
        {
            ErrorMessage = "Must be a valid domain name.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || (value is string str && str.IsNullOrEmpty())) return null;
            if (value != null && value is string strValue)
            {
                if (!strValue.IsNullOrEmpty())
                    if (Regex.IsMatch(strValue, "^(?!:\\/\\/)(?=.{1,255}$)((.{1,63}\\.){1,127}(?![0-9]*$)[a-z0-9-]+\\.?)", RegexOptions.IgnoreCase))
                        return ValidationResult.Success;

            }
            return new ValidationResult(GetErrorMessage(validationContext));


        }
    }
}
