using BLAZAM.Common.Data.Validators;
using System.ComponentModel.DataAnnotations;

namespace BLAZAM.Common.Data.Tests.Validators
{
    public class ValidPortAttributeTests
    {
        private readonly ValidPortAttribute _validator = new ValidPortAttribute();

        [Theory]
        [InlineData(1)]
        [InlineData(80)]
        [InlineData(443)]
        [InlineData(65535)]
        [InlineData(1024)]
        public void IsValid_ShouldReturnSuccess_ForValidPorts(int validPort)
        {
            var result = _validator.GetValidationResult(validPort, new ValidationContext(new object()));
            Assert.Equal(ValidationResult.Success, result);
        }

        [Theory]
        [InlineData(0)]       // Below minimum
        [InlineData(65536)]   // Above maximum
        [InlineData(-1)]      // Negative
        public void IsValid_ShouldReturnError_ForInvalidPorts(int invalidPort)
        {
            var result = _validator.GetValidationResult(invalidPort, new ValidationContext(new object()));
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.NotNull(result);
            Assert.Equal("Enter a port number from 1 to 65535", result.ErrorMessage);
        }

        [Fact]
        public void IsValid_ShouldReturnError_ForNonIntegerValues()
        {
            var result = _validator.GetValidationResult("not a port", new ValidationContext(new object()));
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.NotNull(result);
            Assert.Equal("Enter a port number from 1 to 65535", result.ErrorMessage);
        }

        [Fact]
        public void IsValid_ShouldReturnError_ForNullValue()
        {
            // Null values are typically handled by [Required] attribute if the field is mandatory.
            // This validator will fail if a null is passed directly and not caught by [Required].
            var result = _validator.GetValidationResult(null, new ValidationContext(new object()));
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.NotNull(result);
            Assert.Equal("Enter a port number from 1 to 65535", result.ErrorMessage);
        }


        [Fact]
        public void ErrorMessage_ShouldBeSetCorrectlyInConstructor()
        {
            // Accessing the protected ErrorMessageString requires a bit of a workaround if not using reflection
            // or we can just test the outcome of IsValid for an invalid value.
            var attribute = new ValidPortAttribute();
            var result = attribute.IsValid(0); // Invalid value to trigger error message
            Assert.False(result);
        }

        [Fact]
        public void ErrorMessageResourceName_ShouldBeSet()
        {
            Assert.Equal("ValidPortAttribute", _validator.ErrorMessageResourceName);
        }
    }
}