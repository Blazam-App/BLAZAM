
using System.Security;
using BLAZAM.ActiveDirectory.Data;
using BLAZAM.Helpers;

namespace BLAZAM.Tests.ActiveDirectory
{
    public class DataTests
    {
        [Fact]
        public void LapsCredential_Constructor_ParsesJsonCorrectly()
        {
            // Arrange
            // Example JSON: {"n":"TestAccount","t":"01D9F7A2B6C0A000","p":"TestPassword"}
            string json = "{\"n\":\"TestAccount\",\"t\":\"01D9F7A2B6C0A000\",\"p\":\"TestPassword\"}";
            SecureString secureJson = new SecureString();
            foreach (char c in json)
                secureJson.AppendChar(c);

            // Act
            var credential = new LapsCredential(secureJson);

            // Assert
            Assert.Equal("TestAccount", credential.AccountName.ToPlainText());
            Assert.Equal("TestPassword", credential.Password.ToPlainText());
            // Check CreationTime is parsed correctly
            long fileTime = long.Parse("01D9F7A2B6C0A000", System.Globalization.NumberStyles.HexNumber);
            var expectedDate = DateTime.FromFileTimeUtc(fileTime);
            Assert.Equal(expectedDate, credential.CreationTime);
        }
    }
}