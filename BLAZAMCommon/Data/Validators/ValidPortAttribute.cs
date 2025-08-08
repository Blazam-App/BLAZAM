using System.ComponentModel.DataAnnotations;

namespace BLAZAM.Common.Data.Validators
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ValidPortAttribute : AppValidationAttribute
    {
        public ValidPortAttribute()
        {
            ErrorMessageResourceName = GetType().Name;

            this.ErrorMessage = "Enter a port number from 1 to 65535";
        }

        /// <summary>
        /// Checks if the given integer is from 1 to 65535.
        /// </summary>
        /// <remarks>
        /// <param name="value">Port number to test</param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {

            if (value is int intValue)
            {
                if (intValue > 0 && intValue < 65536)
                    return ValidationResult.Success;

            }

            return new ValidationResult(GetErrorMessage(validationContext));


        }
    }
}
