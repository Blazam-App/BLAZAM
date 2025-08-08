using BLAZAM.Helpers; // Assuming this is the namespace for GuiHelpers
using Microsoft.AspNetCore.Components.Forms;
using Moq;
using System.Text;
// Add using statements for IApplicationUserState, ActiveDirectoryUserState, NewsItem, AppDialogService, IDialogReference, TreeItemData, etc.
// For example:
// using ApplicationNews;
// using BLAZAM.ActiveDirectory.Data;
// using BLAZAM.Gui.UI.Modals; // For AppDialogService, AppNewsItemDialog
// using MudBlazor;

namespace BLAZAM.Tests.Gui
{
    public class GuiHelpersTests
    {
        [Fact]
        public async Task ReadByteArrayAsync_ReadsFileContentCorrectly()
        {
            // Arrange
            var mockFile = new Mock<IBrowserFile>();
            var fileContent = "Hello, world!";
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);
            var memoryStream = new MemoryStream(fileBytes);

            mockFile.Setup(f => f.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                    .Returns(memoryStream);
            mockFile.Setup(f => f.Size).Returns(fileBytes.Length);


            // Act
            var resultBytes = await mockFile.Object.ReadByteArrayAsync(); //

            // Assert
            Assert.NotNull(resultBytes);
            Assert.Equal(fileBytes, resultBytes);
        }

        [Fact]
        public async Task ReadByteArrayAsync_RespectsMaxReadBytes()
        {
            // Arrange
            var mockFile = new Mock<IBrowserFile>();
            var fileContent = "This is a test string that is longer than the max read bytes.";
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);
            var memoryStream = new MemoryStream(fileBytes);
            int maxReadBytes = 10;


            mockFile.Setup(f => f.OpenReadStream(maxReadBytes, It.IsAny<CancellationToken>()))
                    .Returns(() => memoryStream); // Return a new stream each time


            // Act
            // We expect the OpenReadStream to be called with maxReadBytes
            // The actual enforcement of limiting the read bytes would happen inside OpenReadStream or CopyToAsync
            // This test primarily verifies that OpenReadStream is called with the specified limit.
            var resultBytes = await mockFile.Object.ReadByteArrayAsync(maxReadBytes); //


            // Assert
            // The result will be the full stream content because the helper method itself doesn't truncate
            // based on maxReadBytes after OpenReadStream. The maxReadBytes is passed to OpenReadStream.
            // If OpenReadStream itself truncates, then this assertion would be different.
            // For this helper, it reads whatever the stream (potentially limited by OpenReadStream) gives.
            Assert.NotNull(resultBytes);
            Assert.Equal(fileBytes.Length, resultBytes.Length); // It will read the whole array provided by the mock stream
            mockFile.Verify(f => f.OpenReadStream(maxReadBytes, It.IsAny<CancellationToken>()), Times.Once);
        }


        //[Fact]
        //public void ToActiveDirectoryUserState_MapsCorrectly()
        //{
        //    // Arrange
        //    var mockAppUserState = new Mock<IApplicationUserState>();
        //    mockAppUserState.Setup(u => u.AuditUsername).Returns("testuser");
        //    mockAppUserState.Setup(u => u.IsSuperAdmin).Returns(true);
        //    var permissionMappings = new Dictionary<string, object>(); // Populate if needed
        //    mockAppUserState.Setup(u => u.PermissionMappings).Returns(permissionMappings);

        //    // Act
        //    var adUserState = mockAppUserState.Object.ToActiveDirectoryUserState(); //

        //    // Assert
        //    Assert.NotNull(adUserState);
        //    Assert.Equal("testuser", adUserState.Username);
        //    Assert.True(adUserState.IsSuperAdmin);
        //    Assert.Same(permissionMappings, adUserState.PermissionMappings);
        //}

        //[Fact]
        //public async Task ShowNewsItemDialog_CallsDialogServiceWithMessage()
        //{
        //    // Arrange
        //    var mockDialogService = new Mock<AppDialogService>(); // Assuming AppDialogService can be mocked
        //    var newsItem = new NewsItem { Title = "Test News" }; // Populate NewsItem as needed
        //    var mockDialogReference = new Mock<IDialogReference>();

        //    mockDialogService.Setup(ds => ds.ShowMessage<AppNewsItemDialog>(
        //                             It.IsAny<DialogParameters>(),
        //                             newsItem.Title,
        //                             null, // Default title
        //                             It.IsAny<DialogOptions>()))
        //                     .ReturnsAsync(mockDialogReference.Object);

        //    // Act
        //    var result = await newsItem.ShowNewsItemDialog(mockDialogService.Object); //

        //    // Assert
        //    mockDialogService.Verify(ds => ds.ShowMessage<AppNewsItemDialog>(
        //        It.Is<DialogParameters>(dp => dp.Get<NewsItem>("Item") == newsItem),
        //        newsItem.Title,
        //        null,
        //        It.Is<DialogOptions>(opt => opt.MaxWidth == MaxWidth.ExtraExtraLarge && opt.CloseButton == true && opt.CloseOnEscapeKey == true)),
        //        Times.Once);
        //    Assert.Same(mockDialogReference.Object, result);
        //}

        [Fact]
        public void ToTreeItemData_ConvertsIEnumerableToTreeItemDataList()
        {
            // Arrange
            var stringList = new List<string> { "item1", "item2", "item3" };

            // Act
            var treeData = stringList.ToTreeItemData(); //

            // Assert
            Assert.NotNull(treeData);
            Assert.Equal(stringList.Count, treeData.Count);
            for (int i = 0; i < stringList.Count; i++)
            {
                Assert.Equal(stringList[i], treeData[i].Value);
                Assert.Null(treeData[i].Text); // Assuming Text is not set by default
                Assert.Null(treeData[i].Children); // Assuming no children by default
            }
        }

        [Fact]
        public void ToTreeItemData_WithEmptyList_ReturnsEmptyTreeItemDataList()
        {
            // Arrange
            var emptyList = new List<int>();

            // Act
            var treeData = emptyList.ToTreeItemData(); //

            // Assert
            Assert.NotNull(treeData);
            Assert.Empty(treeData);
        }
    }
}