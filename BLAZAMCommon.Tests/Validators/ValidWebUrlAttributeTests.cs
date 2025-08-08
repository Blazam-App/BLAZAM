using System.ComponentModel.DataAnnotations;
using BLAZAM.Common.Data.Validators;

namespace BLAZAM.Common.Data.Tests.Validators
{
    public class ValidWebUrlAttributeTests
    {
        private readonly ValidWebUrlAttribute _validator = new ValidWebUrlAttribute();

        [Theory]
        [InlineData("http://example.com")]
        [InlineData("https://example.com")]
        [InlineData("https://www.example.com")]
        [InlineData("http://example.com/path")]
        [InlineData("https://example.com/path?query=value")]
        [InlineData("http://localhost")]
        [InlineData("https://localhost:5001")]
        [InlineData("http://192.168.1.1")]
        [InlineData("https://10.0.0.1:8080/more/paths?id=1&name=test#fragment")]
        public void IsValid_ShouldReturnSuccess_ForValidWebUrls(string validUrl)
        {
            var result = _validator.GetValidationResult(validUrl, new ValidationContext(new object()));
            Assert.Equal(ValidationResult.Success, result);
        }

        [Theory]
        [InlineData("example.com")]                 // No scheme
        [InlineData("ftp://example.com")]           // Invalid scheme (ftp)
        [InlineData("gopher://example.com")]        // Invalid scheme
        [InlineData("http:/example.com")]           // Malformed scheme (single slash)
        [InlineData("https//example.com")]          // Malformed scheme (no colon)
        [InlineData("http://")]                     // Scheme only
        [InlineData(" justsometext ")]             // Not a URL
        [InlineData("www.example.com")]             // No scheme
        [InlineData(null)]                          // Null value
        [InlineData("")]                            // Empty string
        [InlineData("http://example .com")]         // Space in domain
        public void IsValid_ShouldReturnError_ForInvalidWebUrls(string invalidUrl)
        {
            var result = _validator.GetValidationResult(invalidUrl, new ValidationContext(new object()));
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.NotNull(result);
            Assert.Equal("Must be a valid web address.", result.ErrorMessage);
        }

        [Fact]
        public void IsValid_ShouldReturnError_ForNonStringValue()
        {
            var result = _validator.GetValidationResult(123, new ValidationContext(new object()));
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.NotNull(result);
            Assert.Equal("Must be a valid web address.", result.ErrorMessage);
        }
    }
}