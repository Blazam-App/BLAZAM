using BLAZAM.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace BLAZAM.Common.Data.Validators
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ValidFqdnAttribute : AppValidationAttribute
    {
        public ValidFqdnAttribute()
        {
            ErrorMessageResourceName = GetType().Name;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || (value is string str && str.IsNullOrEmpty())) return null;
            if (value is string strValue)
            {
                if (!strValue.IsNullOrEmpty())
                    if (Regex.IsMatch(strValue,
                        "^(?!:\\/\\/)(?=.{1,255}$)(([a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\\.){1,127}(?![0-9]*$)([a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?))$",
                        RegexOptions.IgnoreCase))
                        return ValidationResult.Success;

            }
            return new ValidationResult(GetErrorMessage(validationContext));


        }
    }
}
