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
            
            await OpenUserMenu();
            await OpenNotificationSettings();
            await CloseDialog();

            await OpenRecycleBin();

            await OpenConfigureSubMenu();

            
            // Expects the URL to contain intro.
            //await Expect(Page).ToHaveURLAsync(new Regex(".*home"));
        }
           [Test]
        public async Task MainMenuTest()
        {
            await LogIn();

          
            await OpenRecycleBin();

            await OpenConfigureSubMenu();

            await OpenSettingsPages();

            await OpenManageNotifications();

            await OpenPermissions();

            await OpenFields();

            await OpenTemplates();

            
            // Expects the URL to contain intro.
            //await Expect(Page).ToHaveURLAsync(new Regex(".*home"));
        }

        private async Task CloseDialog()
        {
            var closeButton = Page.GetByRole(AriaRole.Button, new() { Name= "Close dialog" });

            await Expect(closeButton).ToBeVisibleAsync();
            await Expect(closeButton).ToBeEnabledAsync();
            await closeButton.ClickAsync();
            await Expect(closeButton).ToBeHiddenAsync();
        }
        private async Task OpenRecycleBin()
        {
            var recycleButton = Page.Locator("text=Recycle Bin");

            await Expect(recycleButton).ToBeVisibleAsync();
            await Expect(recycleButton).ToBeEnabledAsync();
            await recycleButton.ClickAsync();

            var recycleHeader = Page.Locator("text=RESTORE SELECTED");

            await Expect(recycleHeader).ToBeVisibleAsync();
        }
        private async Task OpenConfigureSubMenu()
        {
            var recycleButton = Page.Locator("text=Configure");

            await Expect(recycleButton).ToBeVisibleAsync();
            await Expect(recycleButton).ToBeEnabledAsync();
            await recycleButton.ClickAsync();

            var recycleHeader = Page.Locator("text=Settings");

            await Expect(recycleHeader).ToBeVisibleAsync();
        } 
        
        private async Task OpenManageNotifications()
        {
            var recycleButton = Page.Locator("text=Notifications");

            await Expect(recycleButton).ToBeVisibleAsync();
            await Expect(recycleButton).ToBeEnabledAsync();
            await recycleButton.ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex(".*notifications"));


            var recycleHeader = Page.Locator("text=Manage Notifications");

            await Expect(recycleHeader).ToBeVisibleAsync();
        }
        private async Task OpenFields()
        {
            var button = Page.Locator("text=Fields");

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
            var button = Page.Locator("text=Templates");

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
        private async Task OpenPermissions()
        {
            var button = Page.Locator("text=Permissions");

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



            button = Page.Locator("text=MAPPINGS");

            await Expect(button).ToBeVisibleAsync();
            await Expect(button).ToBeEnabledAsync();
            await button.ClickAsync();

            header = Page.Locator("text=OU Privilege Mapper");

            await Expect(header).ToBeVisibleAsync();


            await Task.Delay(500);


          


        }
        private async Task OpenSettingsPages()
        {
            var button = Page.Locator("text=Settings");

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
