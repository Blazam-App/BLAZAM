using BLAZAM.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace BLAZAM.Common.Data.Validators
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ValidIpOrFqdnAttribute : AppValidationAttribute
    {
        public ValidIpOrFqdnAttribute()
        {
            ErrorMessageResourceName = GetType().Name;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || (value is string str && str.IsNullOrEmpty())) return null;
            if (value is string strValue)
            {
                if (!strValue.IsNullOrEmpty())
                    if (Regex.IsMatch(strValue, "^[A-Z]",RegexOptions.IgnoreCase))
                    {
                        if (Regex.IsMatch(strValue, "^(?!:\\/\\/)(?=.{1,255}$)((.{1,63}\\.){1,127}(?![0-9]*$)[a-z0-9-]+\\.?)", RegexOptions.IgnoreCase))
                        {
                            return ValidationResult.Success;
                        }


                    }
                    else
                    {
                        if (Regex.IsMatch(strValue, "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])", RegexOptions.IgnoreCase))
                        {
                            return ValidationResult.Success;
                        }
                    }


            }
            return new ValidationResult(GetErrorMessage(validationContext));


        }
    }
}
