using BLAZAM.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace BLAZAM.Common.Data.Validators
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ValidIpAttribute : AppValidationAttribute
    {
        public ValidIpAttribute()
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
                    @"^((25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9][0-9]?|0)\.){3}(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9][0-9]?|0)$",
                    RegexOptions.IgnoreCase))
                        return ValidationResult.Success;

            }
            return new ValidationResult(GetErrorMessage(validationContext));


        }
    }
}
