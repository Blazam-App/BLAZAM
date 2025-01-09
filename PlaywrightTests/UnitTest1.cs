using Microsoft.Playwright;

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

        [Test]
        public async Task UserMenuTest()
        {
            await LogIn();

            await OpenUserMenu();
            await OpenProfileSettings();
            await CloseDialog();


            // Expects the URL to contain intro.
            //await Expect(Page).ToHaveURLAsync(new Regex(".*home"));
        }
        [Test]
        public async Task MainMenuTest()
        {
            await LogIn();


            await OpenRecycleBin();

            await OpenConfigureSubMenu();


            await OpenManageNotifications();


            await OpenFields();

            await OpenTemplates();


            // Expects the URL to contain intro.
            //await Expect(Page).ToHaveURLAsync(new Regex(".*home"));
        }
        [Test]
        public async Task CreateObjectPageViewTest()
        {
            await LogIn();

            await Page.GetByLabel("Toggle Create").ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Create User" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Custom" })).ToBeVisibleAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Create Group" }).ClickAsync();
            await Expect(Page.GetByLabel("Group Name")).ToBeVisibleAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Create OU" }).ClickAsync();
            await Expect(Page.GetByLabel("Organizational Unit Name")).ToBeVisibleAsync();

            // Expects the URL to contain intro.
            //await Expect(Page).ToHaveURLAsync(new Regex(".*home"));
        }
        [Test]
        public async Task AuditPagesTest()
        {
            await LogIn();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Audit" }).ClickAsync();
            //await Expect(Page.GetByText("BeforeAction")).ToBeVisibleAsync();
            //await Expect(Page.GetByText("AfterAction")).ToBeVisibleAsync();
            //await Page.GetByText("Logins").ClickAsync();
            ////await Expect(Page.GetByText("Daily Logins")).ToBeVisibleAsync();
            //await Expect(Page.GetByText("Action", new() { Exact = true })).ToBeVisibleAsync();
            //await Page.GetByText("System").ClickAsync();
            //await Expect(Page.GetByText("Disabled in demo")).ToBeVisibleAsync();
            //await Page.GetByText("Webhooks").Nth(1).ClickAsync();
            //await Expect(Page.GetByText("Last Attempt Timestamp")).ToBeVisibleAsync();

            // Expects the URL to contain intro.
            //await Expect(Page).ToHaveURLAsync(new Regex(".*home"));
        }

        //[Test]
        //public async Task NewsMenuTest()
        //{
        //    await LogIn();
        //    await Page.GetByRole(AriaRole.Toolbar).GetByRole(AriaRole.Button).Nth(3).ClickAsync();
        //    await Page.GetByLabel("Show read").CheckAsync();
        //    await Expect(Page.GetByLabel("Show read")).ToBeCheckedAsync();
        //    await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Blazam News" })).ToBeVisibleAsync();
        //    await Page.Locator(".mud-overlay").ClickAsync();

        //    // Expects the URL to contain intro.
        //    //await Expect(Page).ToHaveURLAsync(new Regex(".*home"));
        //}



        //[Test]
        //public async Task NotificationsPanelTest()
        //{
        //    await LogIn();
        //    await Page.GetByRole(AriaRole.Toolbar).GetByRole(AriaRole.Button).Nth(4).ClickAsync();
        //    await Page.GetByText("Read Notifications", new() { Exact = true }).ClickAsync();
        //    await Page.GetByText("Read Notifications", new() { Exact = true }).ClickAsync();
        //    await Page.Locator("aside").Filter(new() { HasText = "Notifications" }).GetByRole(AriaRole.Button).Nth(2).ClickAsync();

        //    // Expects the URL to contain intro.
        //    //await Expect(Page).ToHaveURLAsync(new Regex(".*home"));
        //}



        //[Test]
        //public async Task SearchFilterTest()
        //{
        //    await LogIn();
        //    await Page.GetByRole(AriaRole.Button, new() { Name = "All" }).ClickAsync();
        //    await Page.Locator("p").Filter(new() { HasTextRegex = new Regex("^User$") }).ClickAsync();
        //    await Page.GetByRole(AriaRole.Button, new() { Name = "User" }).ClickAsync();
        //    await Page.GetByText("Group", new() { Exact = true }).ClickAsync();
        //    await Page.GetByRole(AriaRole.Button, new() { Name = "Group" }).ClickAsync();
        //    await Page.GetByText("OU", new() { Exact = true }).ClickAsync();
        //    await Page.GetByRole(AriaRole.Button, new() { Name = "OU" }).ClickAsync();
        //    await Page.GetByText("Computer").ClickAsync();
        //    await Page.GetByRole(AriaRole.Button, new() { Name = "Computer" }).ClickAsync();
        //    await Page.GetByText("Printer", new() { Exact = true }).ClickAsync();
        //    await Page.GetByRole(AriaRole.Button, new() { Name = "Printer" }).ClickAsync();
        //    await Page.GetByText("BitLocker").ClickAsync();
        //    // Expects the URL to contain intro.
        //    //await Expect(Page).ToHaveURLAsync(new Regex(".*home"));
        //}



        [Test]
        public async Task AboutTest()
        {
            await LogIn();
            await Page.GetByText("BLAZAM " + DateTime.Now.Year).ClickAsync();
            await Expect(Page.GetByText("Founder: Chris Jacobsen")).ToBeVisibleAsync();
            await Expect(Page.GetByText("Dedicated To Maggie")).ToBeVisibleAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Close", Exact = true }).ClickAsync();

            // Expects the URL to contain intro.
            //await Expect(Page).ToHaveURLAsync(new Regex(".*home"));
        }


        private async Task CloseDialog()
        {
            var closeButton = Page.GetByRole(AriaRole.Button, new() { Name = "Close dialog" });

            await Expect(closeButton).ToBeVisibleAsync();
            await Expect(closeButton).ToBeEnabledAsync();
            await closeButton.ClickAsync();
            await Expect(closeButton).ToBeHiddenAsync();
        }
        private async Task OpenRecycleBin()
        {
            var recycleButton = Page.GetByRole(AriaRole.Link, new() { Name = "Recycle Bin" });

            await Expect(recycleButton).ToBeVisibleAsync();
            await Expect(recycleButton).ToBeEnabledAsync();
            await recycleButton.ClickAsync();

            var recycleHeader = Page.Locator("text=RESTORE SELECTED");

            await Expect(recycleHeader).ToBeVisibleAsync();
        }
        private async Task OpenConfigureSubMenu()
        {
            var recycleButton = Page.GetByLabel("Toggle Configure");

            await Expect(recycleButton).ToBeVisibleAsync();
            await Expect(recycleButton).ToBeEnabledAsync();
            await recycleButton.ClickAsync();

            var recycleHeader = Page.Locator("text=Settings");

            await Expect(recycleHeader).ToBeVisibleAsync();
        }

        private async Task OpenManageNotifications()
        {
            var recycleButton = Page.GetByRole(AriaRole.Link, new() { Name = "Notifications" });

            await Expect(recycleButton).ToBeVisibleAsync();
            await Expect(recycleButton).ToBeEnabledAsync();
            await recycleButton.ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex(".*notifications"));


            var recycleHeader = Page.Locator("text=Manage Notifications");

            await Expect(recycleHeader).ToBeVisibleAsync();
        }
        private async Task OpenFields()
        {
            var button = Page.GetByRole(AriaRole.Link, new() { Name = "Fields" });

            await Expect(button).ToBeVisibleAsync();
            await Expect(button).ToBeEnabledAsync();
            await button.ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex(".*fields"));

            var header = Page.Locator("text=Field Type");

            await Expect(header).ToBeVisibleAsync();


            await Task.Delay(500);
        }
        private async Task OpenTemplates()
        {
            var button = Page.GetByRole(AriaRole.Link, new() { Name = "Templates" });

            await Expect(button).ToBeVisibleAsync();
            await Expect(button).ToBeEnabledAsync();
            await button.ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex(".*templates"));

            var header = Page.Locator("text=Inheritance Tree");

            await Expect(header).ToBeVisibleAsync();


            await Task.Delay(500);


            //button = Page.Locator("text=ACCESS LEVELS");

            //await Expect(button).ToBeVisibleAsync();
            //await Expect(button).ToBeEnabledAsync();
            //await button.ClickAsync();

            //header = Page.Locator("text=Access levels are a template");

            //await Expect(header).ToBeVisibleAsync();


        }

        [Test]
        public async Task OpenPermissions()
        {
            await LogIn();
            await OpenConfigureSubMenu();

            var button = Page.GetByRole(AriaRole.Link, new() { Name = "Permissions" });

            await Expect(button).ToBeVisibleAsync();
            await Expect(button).ToBeEnabledAsync();
            await button.ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex(".*permissions"));

            var header = Page.Locator("text=Delegates are your approved application users");

            await Expect(header).ToBeVisibleAsync();


            await Task.Delay(500);

            button = Page.Locator("text=ACCESS LEVELS");

            await Expect(button).ToBeVisibleAsync();
            await Expect(button).ToBeEnabledAsync();
            await button.ClickAsync();

            header = Page.Locator("text=Access levels are a template");

            await Expect(header).ToBeVisibleAsync();


            await Task.Delay(500);



            //button = Page.Locator("text=MAPPINGS");

            //await Expect(button).ToBeVisibleAsync();
            //await Expect(button).ToBeEnabledAsync();
            //await button.ClickAsync();

            //header = Page.Locator("text=OU Privilege Mapper");

            //await Expect(header).ToBeVisibleAsync();


            await Task.Delay(500);





        }
        [Test]
        public async Task OpenSettingsPages()
        {
            await LogIn();

            await OpenConfigureSubMenu();

            var button = Page.GetByRole(AriaRole.Link, new() { Name = "Settings" });

            await Expect(button).ToBeVisibleAsync();
            await Expect(button).ToBeEnabledAsync();
            await button.ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex(".*settings"));
            var header = Page.Locator("text=Application Settings");

            await Expect(header).ToBeVisibleAsync();


            await Task.Delay(500);


            button = Page.Locator("text=AUTHENTICATION");

            await Expect(button).ToBeVisibleAsync();
            await Expect(button).ToBeEnabledAsync();
            await button.ClickAsync();

            header = Page.Locator("text=Authentication Settings");

            await Expect(header).ToBeVisibleAsync();


            await Task.Delay(500);


            button = Page.Locator("text=ACTIVE DIRECTORY");

            await Expect(button).ToBeVisibleAsync();
            await Expect(button).ToBeEnabledAsync();
            await button.ClickAsync();

            header = Page.Locator("text=Active Directory Settings");

            await Expect(header).ToBeVisibleAsync();


            await Task.Delay(500);


            button = Page.Locator("text=DATABASE");

            await Expect(button).ToBeVisibleAsync();
            await Expect(button).ToBeEnabledAsync();
            await button.ClickAsync();

            header = Page.Locator("text=Database Status");

            await Expect(header).ToBeVisibleAsync();


            await Task.Delay(500);


            button = Page.Locator("text=EMAIL");

            await Expect(button).ToBeVisibleAsync();
            await Expect(button).ToBeEnabledAsync();
            await button.ClickAsync();

            header = Page.Locator("text=Email Settings");

            await Expect(header).ToBeVisibleAsync();


            await Task.Delay(500);


            button = Page.Locator("text=UPDATE");

            await Expect(button).ToBeVisibleAsync();
            await Expect(button).ToBeEnabledAsync();
            await button.ClickAsync();

            header = Page.Locator("text=Update Settings");

            await Expect(header).ToBeVisibleAsync();


            await Task.Delay(500);


            button = Page.Locator("text=USER ACTIVITY");

            await Expect(button).ToBeVisibleAsync();
            await Expect(button).ToBeEnabledAsync();
            await button.ClickAsync();

            header = Page.Locator("text=User Activity");

            await Expect(header).ToBeVisibleAsync();


            await Task.Delay(500);


            button = Page.Locator("text=SYSTEM");

            await Expect(button).ToBeVisibleAsync();
            await Expect(button).ToBeEnabledAsync();
            await button.ClickAsync();

            header = Page.Locator("text=System Settings");

            await Expect(header).ToBeVisibleAsync();



        }



        private async Task OpenNotificationSettings()
        {
            var notificationSettingsButton = Page.Locator("text=Notification Settings");

            await notificationSettingsButton.ClickAsync();

            var notificationHeader = Page.Locator("text=Effective Notification Settings for");

            await Expect(notificationHeader).ToBeInViewportAsync();
            await Expect(notificationHeader).ToBeVisibleAsync();
            await Expect(notificationHeader).ToBeEnabledAsync();
        }
        private async Task OpenProfileSettings()
        {
            var profileSettingsButton = Page.Locator("text=Profile Settings");

            await profileSettingsButton.ClickAsync();

            var uploadProfileIconButton = Page.Locator("text=Upload Profile Icon");

            await Expect(uploadProfileIconButton).ToBeInViewportAsync();
            await Expect(uploadProfileIconButton).ToBeVisibleAsync();
            await Expect(uploadProfileIconButton).ToBeEnabledAsync();
        }

        private async Task OpenUserMenu()
        {
            var userButton = Page.GetByRole(AriaRole.Img).Locator("text=D");

            // Expect an attribute "to be strictly equal" to the value.
            //await Expect(getStarted).ToHaveAttributeAsync("href", "/docs/intro");

            // Click the get started link.
            await userButton.ClickAsync();

            var profileSettingsButton = Page.Locator("text=Profile Settings");

            await Expect(profileSettingsButton).ToBeInViewportAsync();
            await Expect(profileSettingsButton).ToBeVisibleAsync();
            await Expect(profileSettingsButton).ToBeEnabledAsync();
        }

        private async Task LogIn()
        {
            await using var context = await Browser.NewContextAsync(new()
            {
                IsMobile = false,
                ViewportSize = new ViewportSize() { Width = 1280, Height = 1024 }
            });
            await Page.GotoAsync("https://blazam.azurewebsites.net/home");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Log In To Demo" }).ClickAsync();

            try
            {
                await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Home" })).ToBeVisibleAsync();

            }
            catch
            {
                await Page.GetByRole(AriaRole.Banner).GetByRole(AriaRole.Button).First.ClickAsync();
                await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Home" })).ToBeVisibleAsync();

            }
            return;


        }
    }
}
