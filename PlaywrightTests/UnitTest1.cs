namespace PlaywrightTests
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class Tests : PageTest
    {
        [Test]
        public async Task LandingPageHasDemoLoginButtonAndLogsIntoHome()
        {
            await LogIn();

            // Expects the URL to contain intro.
            await Expect(Page).ToHaveURLAsync(new Regex(".*home"));
        }
       


        private async Task LogIn()
        {
            await Page.GotoAsync("https://blazam.azurewebsites.net/");
            //await Page.GotoAsync("http://localhost/");

            // Expect a title "to contain" a substring.
            await Expect(Page).ToHaveTitleAsync(new Regex("Login"));

            // create a locator
            var loginButton = Page.Locator("text=LOG IN TO DEMO");

            // Expect an attribute "to be strictly equal" to the value.
            //await Expect(getStarted).ToHaveAttributeAsync("href", "/docs/intro");

            // Click the get started link.
            await loginButton.ClickAsync();
        }
    }
}
