using Microsoft.Playwright;

namespace PlaywrightTests
{

    [TestFixture]
    public class WindowsTests : Tests
    {
        protected virtual string BaseUrl { get { return "https://demo.blazam.org"; } }

    }
}
