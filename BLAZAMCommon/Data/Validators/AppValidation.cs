using BLAZAM.Localization;
using System.ComponentModel.DataAnnotations;

namespace BLAZAM.Common.Data.Validators
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class AppValidationAttribute : ValidationAttribute
    {


        protected string? GetErrorMessage(ValidationContext validationContext)
        {
            return ErrorMessage;
        }


        public AppValidationAttribute()
        {
            this.ErrorMessageResourceType = typeof(AppValidationLocalization);
        }

        public AppValidationAttribute(Func<string> errorMessageAccessor) : base(errorMessageAccessor)
        {
            this.ErrorMessageResourceType = typeof(AppValidationLocalization);
        }

        public AppValidationAttribute(string errorMessage) : base(errorMessage)
        {
            this.ErrorMessageResourceType = typeof(AppValidationLocalization);
        }



    }
}
