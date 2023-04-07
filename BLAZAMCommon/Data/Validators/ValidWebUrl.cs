using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BLAZAM.Common.Data.Validators
{
    internal class ValidWebUrl : ValidationAttribute
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
