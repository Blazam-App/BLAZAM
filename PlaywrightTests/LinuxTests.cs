using Microsoft.Playwright;

namespace PlaywrightTests
{

    [TestFixture]
    [NonParallelizable]
    public class LinuxTests : Tests
    {
        protected override string BaseUrl { get { return "https://beta.blazam.org"; } }

    }
}
