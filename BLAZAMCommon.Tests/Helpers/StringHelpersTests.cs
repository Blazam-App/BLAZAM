using System.Text;
using BLAZAM.Helpers; // For StringHelpers
using Microsoft.AspNetCore.Components; // For MarkupString

namespace BLAZAMCommon.Tests.Helpers
{
    public class StringHelpersTests
    {
        #region ToMarkupString Tests
        [Fact]
        public void ToMarkupString_WithCarriageReturnNewLine_ConvertsToBrTag()
        {
            var input = "Hello\r\nWorld";
            var expected = (MarkupString)"Hello<br>World";
            Assert.Equal(expected, input.ToMarkupString());
        }

        [Fact]
        public void ToMarkupString_WithNewLine_ConvertsToBrTag()
        {
            var input = "Hello\nWorld";
            var expected = (MarkupString)"Hello<br>World";
            Assert.Equal(expected, input.ToMarkupString());
        }

        [Fact]
        public void ToMarkupString_WithMixedNewLines_ConvertsAllToBrTags()
        {
            var input = "Hello\r\nCruel\nWorld";
            var expected = (MarkupString)"Hello<br>Cruel<br>World";
            Assert.Equal(expected, input.ToMarkupString());
        }

        [Fact]
        public void ToMarkupString_WithNoNewLines_ReturnsOriginalMarkupString()
        {
            var input = "HelloWorld";
            var expected = (MarkupString)"HelloWorld";
            Assert.Equal(expected, input.ToMarkupString());
        }

        [Fact]
        public void ToMarkupString_EmptyString_ReturnsEmptyMarkupString()
        {
            var input = "";
            var expected = (MarkupString)"";
            Assert.Equal(expected, input.ToMarkupString());
            Assert.True(input.ToMarkupString().Value.Length == 0);
        }

        [Fact]
        public void ToMarkupString_NullString_ReturnsEmptyMarkupString()
        {
            string? input = null;
            var expected = (MarkupString)"";
            Assert.Equal(expected, input.ToMarkupString());
            Assert.True(input.ToMarkupString().Value.Length == 0);
        }
        #endregion ToMarkupString Tests

        #region GetAppHashCode Tests
        [Fact]
        public void GetAppHashCode_DifferentStrings_ReturnDifferentHashCodes()
        {
            var hash1 = "string1".GetAppHashCode();
            var hash2 = "string2".GetAppHashCode();
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void GetAppHashCode_SameString_ReturnsConsistentHashCode()
        {
            var hash1 = "testString".GetAppHashCode();
            var hash2 = "testString".GetAppHashCode();
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void GetAppHashCode_EmptyString_ReturnsSpecificHashCode()
        {
            // The specific hash code for an empty string is deterministic by the algorithm
            // Algorithm: hash = 17; (no loop iterations) -> returns 17
            Assert.Equal(17, "".GetAppHashCode());
        }

        [Fact]
        public void GetAppHashCode_NullString_ReturnsZero()
        {
            string? input = null;
            Assert.Equal(0, input.GetAppHashCode());
        }

        [Fact]
        public void GetAppHashCode_Permutations_ReturnDifferentHashCodes()
        {
            var hash1 = "ab".GetAppHashCode();
            var hash2 = "ba".GetAppHashCode();
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void GetAppHashCode_VeryLongString_ReturnsAHashCode()
        {
            var longString = new string('a', 10000);
            var hash = longString.GetAppHashCode();
            // We just need to ensure it computes without error and returns something.
            // The actual value could be anything, but for consistency:
            // hash = 17; foreach char 'a': hash = hash * 23 + 'a' (97)
            // This will produce a consistent large number (possibly overflowing to negative)
            int expectedHash = 17;
            foreach (char c in longString)
            {
                unchecked { expectedHash = expectedHash * 23 + c; }
            }
            Assert.Equal(expectedHash, hash);
        }
        #endregion GetAppHashCode Tests

        #region IsUrlLocalToHost Tests
        [Theory]
        [InlineData("/path/to/page", true)]       // Relative path
        [InlineData("~/path/to/page", true)]      // Tilde slash path
        [InlineData("/", true)]                   // Root path
        [InlineData("~/", true)]                  // Tilde slash root
        [InlineData("~", false)]                  // Tilde alone (not matching ~/ or /)
        [InlineData("", true)]                    // Empty string
        [InlineData(null, true)]                  // Null string
        [InlineData("https://localhost/path", true)] // HTTPS localhost
        [InlineData("http://localhost/path", false)]// HTTP localhost (implementation only checks https)
        [InlineData("//otherdomain.com/path", false)]// Protocol-relative external
        [InlineData("http://example.com", false)] // Absolute external HTTP
        [InlineData("https://example.com", false)]// Absolute external HTTPS
        [InlineData("www.example.com", false)]    // Schemeless external
        [InlineData("/test?query=value", true)]   // Relative with query string
        [InlineData("~/test#fragment", true)]     // Tilde slash with fragment
        [InlineData("/\\invalid", false)]         // Forward slash followed by backslash
        public void IsUrlLocalToHost_VariousUrls_ReturnsExpected(string url, bool expected)
        {
            Assert.Equal(expected, url.IsUrlLocalToHost());
        }
        #endregion IsUrlLocalToHost Tests

        #region ToGuid Tests
        [Fact]
        public void ToGuid_SameString_GeneratesSameGuid()
        {
            var input = "TestStringForGuid";
            var guid1 = input.ToGuid();
            var guid2 = input.ToGuid();
            Assert.Equal(guid1, guid2);
        }

        [Fact]
        public void ToGuid_DifferentStrings_GeneratesDifferentGuids()
        {
            var guid1 = "String1ForGuid".ToGuid();
            var guid2 = "String2ForGuid".ToGuid();
            Assert.NotEqual(guid1, guid2);
        }

        [Fact]
        public void ToGuid_EmptyString_GeneratesSpecificGuid()
        {
            // MD5 of empty string is d41d8cd98f00b204e9800998ecf8427e
            var expectedGuid = new Guid("d41d8cd9-8f00-b204-e980-0998ecf8427e"); // Default encoding dependent
            // To make it more robust, compute it like the helper
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(""));
                expectedGuid = new Guid(hash);
            }
            Assert.Equal(expectedGuid, "".ToGuid());
        }

        [Fact]
        public void ToGuid_NullString_ThrowsArgumentNullException()
        {
            string? input = null;
            Assert.Throws<ArgumentNullException>(() => input.ToGuid());
        }
        #endregion ToGuid Tests
    }
}
