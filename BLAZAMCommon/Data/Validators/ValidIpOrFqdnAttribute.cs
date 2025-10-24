using BLAZAM.Helpers;
using System.ComponentModel.DataAnnotations;

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
            if (value == null || (value is string str && str.IsNullOrEmpty()))
            {
                return null;
            }

            if (value is string strValue && !strValue.IsNullOrEmpty())
            {
                var ipValidator = new ValidIpAttribute();
                var fqdnValidator = new ValidFqdnAttribute();
                if (ipValidator.IsValid(strValue) || fqdnValidator.IsValid(strValue))
                {
                    return ValidationResult.Success;
                }
            }
            return new ValidationResult(GetErrorMessage(validationContext));


        }
    }
}
