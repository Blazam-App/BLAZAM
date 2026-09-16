using Microsoft.Playwright;

namespace PlaywrightTests
{

    [TestFixture]
    [NonParallelizable]
    public class WindowsTests : Tests
    {
        protected virtual string BaseUrl { get { return "https://demo.blazam.org"; } }

    }
}
