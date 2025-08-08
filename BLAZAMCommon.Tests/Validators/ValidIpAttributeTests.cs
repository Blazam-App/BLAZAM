using BLAZAM.Common.Data.Validators;
using System.ComponentModel.DataAnnotations;

namespace BLAZAM.Common.Data.Tests.Validators
{
    public class ValidIpAttributeTests
    {
        private readonly ValidIpAttribute _validator = new ValidIpAttribute();

        [Theory]
        [InlineData("192.168.1.1")]
        [InlineData("10.0.0.255")]
        [InlineData("0.0.0.0")]
        [InlineData("255.255.255.255")]
        [InlineData("1.2.3.4")]
        public void IsValid_ShouldReturnSuccess_ForValidIpAddresses(string validIp)
        {
            var result = _validator.GetValidationResult(validIp, new ValidationContext(new object()));
            Assert.Equal(ValidationResult.Success, result);
        }

        [Theory]
        [InlineData("256.168.1.1")]    // Segment out of range
        [InlineData("192.256.1.1")]    // Segment out of range
        [InlineData("192.168.256.1")]    // Segment out of range
        [InlineData("192.168.1.256")]    // Segment out of range
        [InlineData("192.168.1")]      // Incomplete IP
        [InlineData("192.168.1.1.1")]  // Too many segments
        [InlineData("abc.def.ghi.jkl")]// Not an IP
        [InlineData("192.168.1.01")]   // Segment with leading zero (regex might allow, but often considered invalid style) - Current regex allows
        [InlineData("example.com")]    // FQDN
        [InlineData("192.168..1")]     // Double dot
        public void IsValid_ShouldReturnError_ForInvalidIpAddresses(string invalidIp)
        {
            var result = _validator.GetValidationResult(invalidIp, new ValidationContext(new object()));
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.NotNull(result);
            Assert.NotEmpty(result.ErrorMessage);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void IsValid_ShouldReturnNull_ForNullOrEmptyStrings(string ip)
        {
            var result = _validator.GetValidationResult(ip, new ValidationContext(new object()));
            Assert.Null(result);
        }

        [Fact]
        public void ErrorMessageResourceName_ShouldBeSet()
        {
            Assert.Equal("ValidIpAttribute", _validator.ErrorMessageResourceName);
        }
    }
}