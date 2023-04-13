using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace BLAZAM.Common.Data.Validators
{
    public class ValidAdminPassword : ValidationAttribute
    {
        /// <summary>
        /// Checks if the given string meets minimum admin password complexity.
        /// </summary>
        /// <remarks>
        /// Min complexity: 6 character min, has at least one leter, number, and special character.
        /// </remarks>
        /// <param name="value">Password to test</param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string strValue)
            {
                Regex regex = new Regex(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{6,}$");
                if(regex.Match(strValue).Success)
                    return ValidationResult.Success;

            }

            return new ValidationResult("Doesn't meet complexity requirements!");


        }
    }
}
