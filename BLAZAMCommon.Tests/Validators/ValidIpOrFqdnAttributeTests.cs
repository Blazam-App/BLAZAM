using BLAZAM.Common.Data.Validators;
using System.ComponentModel.DataAnnotations;

namespace BLAZAM.Common.Data.Tests.Validators
{
    public class ValidIpOrFqdnAttributeTests
    {
        private readonly ValidIpOrFqdnAttribute _validator = new ValidIpOrFqdnAttribute();

        [Theory]
        [InlineData("192.168.1.1")]         // Valid IP
        [InlineData("10.0.0.255")]          // Valid IP
        [InlineData("example.com")]         // Valid FQDN
        [InlineData("sub.example.co.uk")]   // Valid FQDN
        [InlineData("test-domain.org")]     // Valid FQDN
        public void IsValid_ShouldReturnSuccess_ForValidInputs(string validInput)
        {
            var result = _validator.GetValidationResult(validInput, new ValidationContext(new object()));
            Assert.Equal(ValidationResult.Success, result);
        }

        [Theory]
        [InlineData("256.10.10.1")]        // Invalid IP
        [InlineData("192.168.1")]          // Incomplete IP
        [InlineData("http://example.com")] // FQDN with protocol (invalid for this validator's FQDN check)
        [InlineData("example")]            // Not a valid FQDN or IP
        [InlineData("example..com")]       // Invalid FQDN
        [InlineData("-example.com")]      // Invalid FQDN
        [InlineData("example.123")]        // Invalid FQDN (TLD all numbers)
        public void IsValid_ShouldReturnError_ForInvalidInputs(string invalidInput)
        {
            var result = _validator.GetValidationResult(invalidInput, new ValidationContext(new object()));
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.NotNull(result);
            Assert.NotEmpty(result.ErrorMessage);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void IsValid_ShouldReturnNull_ForNullOrEmptyStrings(string input)
        {
            var result = _validator.GetValidationResult(input, new ValidationContext(new object()));
            Assert.Null(result);
        }

        [Fact]
        public void ErrorMessageResourceName_ShouldBeSet()
        {
            Assert.Equal("ValidIpOrFqdnAttribute", _validator.ErrorMessageResourceName);
        }
    }
}