using Xunit;
using Moq;
using BLAZAM.Helpers;
using BLAZAM.Common.Data;
using MudBlazor;
using BLAZAM.ActiveDirectory.Interfaces; // Assuming this is the namespace for DirectoryAdapterHelpers
// Add using statements for IDirectoryEntryAdapter, ActiveDirectoryObjectType, and MudBlazor Icons
// For example:
// using BLAZAM.ActiveDirectory; // Or wherever IDirectoryEntryAdapter is defined
// using MudBlazor; // For Icons

namespace BLAZAM.Tests.Gui
{
    public class DirectoryAdapterHelpersTests
    {
        [Fact]
        public void TypeIcon_ForIDirectoryEntryAdapter_ReturnsCorrectIcon()
        {
            // Arrange
            var mockAdapter = new Mock<IDirectoryEntryAdapter>();
            mockAdapter.Setup(a => a.ObjectType).Returns(ActiveDirectoryObjectType.User); // Example

            // Act
            var icon = mockAdapter.Object.TypeIcon();

            // Assert
            Assert.Equal(Icons.Material.Filled.Person, icon); //
        }

        [Theory]
        [InlineData(ActiveDirectoryObjectType.User, Icons.Material.Filled.Person)]
        [InlineData(ActiveDirectoryObjectType.Group, Icons.Material.Filled.Group)]
        [InlineData(ActiveDirectoryObjectType.Computer, Icons.Material.Filled.Computer)]
        [InlineData(ActiveDirectoryObjectType.OU, Icons.Material.Filled.Folder)]
        [InlineData(ActiveDirectoryObjectType.Printer, Icons.Material.Filled.Print)]
        [InlineData(ActiveDirectoryObjectType.Contact, Icons.Material.Filled.Contacts)]
        [InlineData(ActiveDirectoryObjectType.BitLocker, Icons.Material.Filled.EnhancedEncryption)]
        [InlineData(ActiveDirectoryObjectType.All, Icons.Material.Filled.AccountTree)]
        [InlineData((ActiveDirectoryObjectType)99, Icons.Material.Filled.QuestionMark)] // Test default case
        public void TypeIcon_ForActiveDirectoryObjectType_ReturnsCorrectIcon(ActiveDirectoryObjectType type, string expectedIcon)
        {
            // Act
            var icon = type.TypeIcon(); //

            // Assert
            Assert.Equal(expectedIcon, icon);
        }
    }
}