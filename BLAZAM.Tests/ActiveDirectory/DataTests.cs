
using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Data;
using BLAZAM.Tests.Mocks;
using BLAZAM.Update;

namespace BLAZAM.Tests.ActiveDirectory
{
    public class DataTests
    {
     
        [Fact]
        public async Task FormatLapsJson_Parses_Correctly()
        {
            var decryptor = new LapsDecryptor();

            var input = "{\"n\":\"Administrator\",\"t\":\"1dbf5e53cfa9dcc\",\"p\":\"f@k34@$$w0rd\"}";

            var output = ADComputer.FormatLAPSJson(input);
            Assert.Equal(output, "Administrator: f@k34@$$w0rd");
        }
        [Fact]
        public async Task FormatLapsJson_Fails_Correctly()
        {
            var decryptor = new LapsDecryptor();

            var input = "";

            Assert.ThrowsAny<Exception>(() => {
            var output = ADComputer.FormatLAPSJson(input);

            });
        }
    }
}
