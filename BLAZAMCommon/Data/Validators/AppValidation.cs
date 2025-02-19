using BLAZAM.Helpers;
using BLAZAM.Localization;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BLAZAM.Common.Data.Validators
{
    public class AppValidationAttribute : ValidationAttribute
    {
  
   
        protected string? GetErrorMessage(ValidationContext validationContext)
        {
            return ErrorMessage;
            return GetLocalizer(validationContext)[ErrorMessage];
        }

        private static IStringLocalizer localizer;
        private IStringLocalizer GetLocalizer(ValidationContext validationContext)
        {
            if (localizer is null)
            {
                var factory = validationContext.GetRequiredService<IStringLocalizer<AppLocalization>>();

            }

            return localizer;
        }

    }
}
