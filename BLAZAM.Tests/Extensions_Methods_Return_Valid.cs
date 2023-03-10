using static BLAZAM.CommonExtensions;

namespace BLAZAM.Tests
{
    public class Extensions_Methods_Return_Valid
    {
        [Fact]
        public void ToSecureString_Then_ToPlainText_Returns_Same()
        {
            var test = "test";
            var encoded = test.ToSecureString();
            var decoded = encoded.ToPlainText();
            bool result = decoded.Equals(test);

            Assert.True(result,"Decrypted secure string should match plain text that was encrypted");
        }
        
        [Fact]
        public void ToSecureString_NotPlainText()
        {
            var test = "test";
            var encoded = test.ToSecureString();
            bool result = test.Equals(encoded.ToString());

            Assert.False(result, "Encrypted strings should not match plain text that was encrypted");
        }

        [Fact]
        public void ToPrettyOU_ReturnsValid()
        {
            var test = "OU=test,OU=ou,DC=test,DC=dc";
            var valid = "ou/test";
            var pretty = test.ToPrettyOu();
            bool result = pretty.Equals(valid);

            Assert.True(result, "The pretty ou format returned was not correct. "+ test + " should be formatted as "+ valid);
        }

        [Fact]
        public void FqdnToDN_ReturnsValid()
        {
            var test = "my.long.test.Domain.name.com";
            var valid = "DC=my,DC=long,DC=test,DC=Domain,DC=name,DC=com";
            var dn = test.FqdnToDN();
            bool result = dn.Equals(valid);

            Assert.True(result, "The fqdn " + test + " should return a DN of " + valid);
        }
        //3/17/2023 12:00:00 AM
        /*
        [Fact]
        public void DateTimeToAdsValue_ReturnsValid()
        {
            var valid = new ADsLargeInteger();
            valid.HighPart = 31021155;
            valid.LowPart = 1790853120;
            DateTime? test = DateTime.Parse("3/17/2023 12:00:00 AM");
            var converted = test.DateTimeToAdsValue();
            bool result = converted.Equals(valid);

            Assert.True(result, "The conversion of COM large integers to DateTime is not returning the correct DateTime");
        }

        [Fact]
        public void AdsValueToDateTime_ReturnsValid()
        {
            var test = new ADsLargeInteger();
            test.HighPart = 31021155;
            test.LowPart = 1790853120;
            var valid = DateTime.Parse("3/17/2023 12:00:00 AM");
            var converted = test.AdsValueToDateTime();
            bool result = converted.Equals(valid);
            
            Assert.True(result, "The conversion of COM large integers to DateTime is not returning the correct DateTime");
        }
        */
    }
}