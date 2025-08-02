using Xunit;
using BLAZAM.Common.Data.Validators;
using System.ComponentModel.DataAnnotations;

namespace BLAZAM.Common.Data.Tests.Validators
{
    public class ValidAdminPasswordAttributeTests
    {
        private readonly ValidAdminPasswordAttribute _validator = new ValidAdminPasswordAttribute();

        [Theory]
        [InlineData("Password123!")] // Meets all criteria
        [InlineData("P@sswOrd1")]    // Meets all criteria, exactly 6 chars
        [InlineData("Secure1#")]     // Meets all criteria
        public void IsValid_ShouldReturnSuccess_ForValidPasswords(string validPassword)
        {
            var result = _validator.GetValidationResult(validPassword, new ValidationContext(new object()));
            Assert.Equal(ValidationResult.Success, result);
        }

        [Theory]
        [InlineData("pass")] // Too short
        [InlineData("Password123")] // No special character
        [InlineData("password!")]   // No number
        [InlineData("PASSWORD!")]   // No number
        [InlineData("12345!")]      // No letter
        [InlineData("Pas1!")]      // Less than 6 characters
        [InlineData("")]            // Empty string
        [InlineData(null)]          // Null value
        public void IsValid_ShouldReturnError_ForInvalidPasswords(string invalidPassword)
        {
            var result = _validator.GetValidationResult(invalidPassword, new ValidationContext(new object()));
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.NotNull(result); // Ensure an error message is provided
            Assert.NotEmpty(result.ErrorMessage);
        }

        [Fact]
        public void ErrorMessageResourceName_ShouldBeSet()
        {
            Assert.Equal("ValidAdminPasswordAttribute", _validator.ErrorMessageResourceName);
        }
    }
}