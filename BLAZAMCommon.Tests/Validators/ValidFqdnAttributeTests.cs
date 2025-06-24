using Xunit;
using BLAZAM.Common.Data.Validators;
using System.ComponentModel.DataAnnotations;

namespace BLAZAM.Common.Data.Tests.Validators
{
    public class ValidFqdnAttributeTests
    {
        private readonly ValidFqdnAttribute _validator = new ValidFqdnAttribute();

        [Theory]
        [InlineData("example.com")]
        [InlineData("sub.example.com")]
        [InlineData("sub.domain.example.co.uk")]
        [InlineData("test-domain.org")]
        [InlineData("a.b")] //Technically valid by some definitions, regex allows it
        [InlineData("domain-with-numbers123.net")]
        public void IsValid_ShouldReturnSuccess_ForValidFqdns(string validFqdn)
        {
            var result = _validator.GetValidationResult(validFqdn, new ValidationContext(new object()));
            Assert.Equal(ValidationResult.Success, result);
        }

        [Theory]
        [InlineData("http://example.com")] // Contains protocol
        [InlineData("example")]             // Not an FQDN
        [InlineData("example..com")]        // Double dot
        [InlineData("-example.com")]       // Starts with hyphen
        [InlineData("example.com-")]       // Ends with hyphen (on TLD)
        [InlineData("example.123")]         // TLD is all numbers (invalid based on regex `(?![0-9]*$)`)
        [InlineData("192.168.1.1")]       // IP address
        [InlineData(".example.com")]       // Starts with a dot
        [InlineData("example.com.")]       // Ends with a dot (though some systems treat this as valid FQDN root) - current regex allows it.
                                           // If this should fail, regex `^(?!:\/\/)(?=.{1,255}$)((.{1,63}\.){1,127}(?![0-9]*$)[a-z0-9-]+(?<!-)\.?)$`

        public void IsValid_ShouldReturnError_ForInvalidFqdns(string invalidFqdn)
        {
            var result = _validator.GetValidationResult(invalidFqdn, new ValidationContext(new object()));
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.NotNull(result);
            Assert.NotEmpty(result.ErrorMessage);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void IsValid_ShouldReturnNull_ForNullOrEmptyStrings(string fqdn)
        {
            // The attribute is designed to return null (effectively success by convention for optional fields)
            // if the value is null or empty, as per:
            // if (value == null || (value is string str && str.IsNullOrEmpty())) return null;
            var result = _validator.GetValidationResult(fqdn, new ValidationContext(new object()));
            Assert.Null(result); // This means success in DataAnnotations when no [Required] is present
        }


        [Fact]
        public void ErrorMessageResourceName_ShouldBeSet()
        {
            Assert.Equal("ValidFqdnAttribute", _validator.ErrorMessageResourceName);
        }
    }
}