using BLAZAM.Common.Data;
using BLAZAM.Database.Models.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Tests.Gui
{



    public class DirectoryTemplateTests
    {
        private readonly DirectoryTemplate _template = new DirectoryTemplate();
        private readonly NewUserName _testUser = new NewUserName
        {
            GivenName = "John",
            MiddleName = "Michael",
            Surname = "Doe"
        };

        [Fact]
        public void ShouldSubstituteFirstName()
        {
            var result = _template.ReplaceVariables("{fn}", _testUser);
            Assert.Equal("John", result);
        }

        [Fact]
        public void ShouldSubstituteFirstInitial()
        {
            var result = _template.ReplaceVariables("{fi}", _testUser);
            Assert.Equal("J", result);
        }

        [Fact]
        public void ShouldSubstituteLastName()
        {
            var result = _template.ReplaceVariables("{ln}", _testUser);
            Assert.Equal("Doe", result);
        }

        [Fact]
        public void ShouldSubstituteLastInitial()
        {
            var result = _template.ReplaceVariables("{li}", _testUser);
            Assert.Equal("D", result);
        }

        [Fact]
        public void ShouldSubstituteMiddleName()
        {
            var result = _template.ReplaceVariables("{mn}", _testUser);
            Assert.Equal("Michael", result);
        }

        [Fact]
        public void ShouldSubstituteMiddleInitial()
        {
            var result = _template.ReplaceVariables("{mi}", _testUser);
            Assert.Equal("M", result);
        }

        [Fact]
        public void ShouldSubstituteUsername()
        {
            // Assuming UsernameFormula is set to something like "{fi}{ln}" for this test
            _template.UsernameFormula = "{fi}{ln}";
            var result = _template.ReplaceVariables("{username}", _testUser);
            Assert.Equal("JDoe", result);
        }

        [Fact]
        public void ShouldSubstituteFirstNameUppercase()
        {
            var result = _template.ReplaceVariables("{fn:u}", _testUser);
            Assert.Equal("JOHN", result);
        }

        [Fact]
        public void ShouldSubstituteFirstNameLowercase()
        {
            var result = _template.ReplaceVariables("{fn:l}", _testUser);
            Assert.Equal("john", result);
        }

        [Fact]
        public void ShouldSubstituteFirstNameWithLength()
        {
            var result = _template.ReplaceVariables("{fn[2]}", _testUser);
            Assert.Equal("Jo", result);
        }

        [Fact]
        public void ShouldSubstituteFirstNameLowercaseWithLength()
        {
            var result = _template.ReplaceVariables("{fn:l[2]}", _testUser);
            Assert.Equal("jo", result);
        }

        [Fact]
        public void ShouldSubstituteLastNameUppercaseWithLength()
        {
            var result = _template.ReplaceVariables("{ln:u[2]}", _testUser);
            Assert.Equal("DO", result);
        }

        [Fact]
        public void ShouldSubstituteLastNameWithRegex()
        {
            // Test with a regex that extracts the first 3 characters and "Jr" if present.
            // Using a simpler regex for testing purposes as the example might be too complex for a unit test.
            var result = _template.ReplaceVariables("{ln:regex[^(.{2})]}", _testUser);
            Assert.Equal("Do", result);
        }

        [Fact]
        public void ShouldSubstituteFirstNameWithRegex()
        {
            // Test with a regex that extracts the first 2 characters
            var result = _template.ReplaceVariables("{fn:regex[^(.{2})]}", _testUser);
            Assert.Equal("Jo", result);
        }

        [Fact]
        public void ShouldSubstituteAlphaNumeric()
        {
            var result = _template.ReplaceVariables("{alphanum}", _testUser);
            Assert.Matches("^[a-zA-Z0-9]$", result);
        }

        [Fact]
        public void ShouldSubstituteAlphaNumericUppercase()
        {
            var result = _template.ReplaceVariables("{alphanum:u}", _testUser);
            Assert.Matches("^[A-Z0-9]$", result); //Should be A-Z or 0-9
        }

        [Fact]
        public void ShouldSubstituteAlphaNumericLowercase()
        {
            var result = _template.ReplaceVariables("{alphanum:l}", _testUser);
            Assert.Matches("^[a-z0-9]$", result); //Should be a-z or 0-9
        }

        [Fact]
        public void ShouldSubstituteAlpha()
        {
            var result = _template.ReplaceVariables("{alpha}", _testUser);
            Assert.Matches("^[a-zA-Z]$", result);
        }

        [Fact]
        public void ShouldSubstituteAlphaUppercase()
        {
            var result = _template.ReplaceVariables("{alpha:u}", _testUser);
            Assert.Matches("^[A-Z]$", result);
        }

        [Fact]
        public void ShouldSubstituteAlphaLowercase()
        {
            var result = _template.ReplaceVariables("{alpha:l}", _testUser);
            Assert.Matches("^[a-z]$", result);
        }

        [Fact]
        public void ShouldSubstituteNumeric()
        {
            var result = _template.ReplaceVariables("{num}", _testUser);
            Assert.Matches("^[0-9]$", result);
        }

        [Fact]
        public void ShouldHandleEmptyUserInputs()
        {
            var emptyUser = new NewUserName(); // All properties are null or empty
            var result = _template.ReplaceVariables("{fn}{ln}{mn}", emptyUser);
            Assert.Equal("", result); // Expect empty string as properties are null

            var resultFi = _template.ReplaceVariables("{fi}", emptyUser);
            Assert.Equal("", resultFi); // Expect empty string as properties are null

            var resultLi = _template.ReplaceVariables("{li}", emptyUser);
            Assert.Equal("", resultLi); // Expect empty string as properties are null

            var resultMi = _template.ReplaceVariables("{mi}", emptyUser);
            Assert.Equal("", resultMi); // Expect empty string as properties are null
        }

        [Fact]
        public void ShouldHandleNullUserInputsForReplaceVariables()
        {
            var result = _template.ReplaceVariables("{fn}{ln}{mn}", null);
            Assert.Equal("", result);

            var resultFi = _template.ReplaceVariables("{fi}", null);
            Assert.Equal("", resultFi);
            var resultLi = _template.ReplaceVariables("{li}", null);
            Assert.Equal("", resultLi);

            var resultMi = _template.ReplaceVariables("{mi}", null);
            Assert.Equal("", resultMi);

        }


        [Fact]
        public void ShouldPreserveUnknownVariables()
        {
            var result = _template.ReplaceVariables("{unknown} {fn}", _testUser);
            Assert.Equal("{unknown} John", result);
        }

        [Fact]
        public void ShouldHandleInvalidModifierSyntaxGracefully()
        {
            // Example of invalid modifier: {fn:x} where x is not a valid modifier
            var result = _template.ReplaceVariables("{fn:x}", _testUser);
            // Current implementation seems to return the original value if modifier is not recognized
            // This might need adjustment based on desired behavior for invalid modifiers
            Assert.Equal("John", result);
        }

        [Fact]
        public void ShouldHandleInvalidLengthSyntaxGracefully()
        {
            var result = _template.ReplaceVariables("{fn[abc]}", _testUser);
            // Current implementation returns full string if argument is not a number
            Assert.Equal("John", result);
        }
        [Fact]
        public void ShouldHandleLengthGreaterThanValueLength()
        {
            var result = _template.ReplaceVariables("{fn[100]}", _testUser);
            // Current implementation returns full string if length is greater than value length
            Assert.Equal("John", result);
        }
    }


}
