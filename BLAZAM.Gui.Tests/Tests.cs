using BLAZAM.Gui;
using BLAZAM.Gui.UI;
using BLAZAM.Localization;
using BLAZAM.Notifications.Services;
using Bunit;
using Bunit.Rendering;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Moq;
using MudBlazor;
using Xunit;

public class Tests : BunitContext
{
    //public Tests()
    //{
    //    // Setup mocks for dependencies
    //    var mockAppLocalization = new Mock<IStringLocalizer<AppLocalization>>();
    //    var mockHelpLocalization = new Mock<IStringLocalizer<AppHelpLocalization>>();
    //    var mockSnackBar = new Mock<ISnackbar>();

    //    Services.AddSingleton(mockAppLocalization.Object);
    //    Services.AddSingleton(mockHelpLocalization.Object);
    //    Services.AddSingleton(new AppSnackBarService(mockSnackBar.Object));
    //}

    //[Fact]
    //public void AppNavLock_WhenLockIsTrue_RendersNavigationLock()
    //{
    //    // Arrange & Act
    //    var cut = Render<AppNavLock>(parameters => parameters
    //        .Add(p => p.Lock, true));

    //    // Assert
    //    var navLock = cut.FindComponent<NavigationLock>();
    //    Assert.NotNull(navLock);
    //}

    //[Fact]
    //public void AppNavLock_WhenLockIsFalse_DoesNotRenderNavigationLock()
    //{
    //    // Arrange & Act
    //    var cut = Render<AppNavLock>(parameters => parameters
    //        .Add(p => p.Lock, false));

    //    // Assert
    //    Assert.Throws<ComponentNotFoundException>(() =>
    //        cut.FindComponent<NavigationLock>());
    //}

    //[Fact]
    //public void AppNavLock_RendersAppModal()
    //{
    //    // Arrange & Act
    //    var cut = Render<AppNavLock>();

    //    // Assert
    //    var modal = cut.FindComponent<AppModal>();
    //    Assert.NotNull(modal);
    //}
}