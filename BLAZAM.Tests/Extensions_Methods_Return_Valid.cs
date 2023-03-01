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
    }
}