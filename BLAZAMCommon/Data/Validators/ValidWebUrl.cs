using System.ComponentModel.DataAnnotations;

namespace BLAZAM.Common.Data.Validators
{
    public class ValidWebUrl : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string strValue)
            {
                Uri uriResult;
                if (Uri.TryCreate(strValue, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                {
                    return ValidationResult.Success;

                }
            }
            return new ValidationResult("Must be a valid web address.");

        }
    }
}
