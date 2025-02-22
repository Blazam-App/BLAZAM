using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace BLAZAM.Common.Data.Validators
{
    public class ValidPort : AppValidationAttribute
    {
        public ValidPort()
        {
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
                if (intValue>0 && intValue<65536)
                    return ValidationResult.Success;

            }

            return new ValidationResult(GetErrorMessage(validationContext));


        }
    }
}
