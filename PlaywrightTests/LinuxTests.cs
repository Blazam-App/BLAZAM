using Microsoft.Playwright;

namespace PlaywrightTests
{

    [TestFixture]
    public class LinuxTests : Tests
    {
        protected virtual string BaseUrl { get { return "https://beta.blazam.org"; } }

    }
}
