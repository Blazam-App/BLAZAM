using BLAZAM.Tests.Mocks;
using BLAZAM.Update;
using BLAZAM.Update.Services;

namespace BLAZAM.Tests.Updates
{
    public class UpdateTests
    {
        private readonly Mock_UpdateService _updateService = new();

        [Fact]
        public async Task Update_Returns_Data()
        {
            var latest = await _updateService.GetUpdates();
            Assert.NotNull(latest);
        }

        [Fact]
        public async Task Updat_Returns_ValidVersion()
        {
            var latest = await _updateService.GetUpdates();
            Assert.NotNull(latest?.Version);
        }

        [Fact]
        public async Task Update_Returns_ValidDownload()
        {
            var latest = await _updateService.GetUpdates();
            if (latest != null)
                await latest.Download(null);
            if (latest == null)
            {
                Assert.NotNull(latest);
            }
            else
            {
                Assert.True(latest.UpdateFile.Exists);
                await Update_Stages_OK(latest);
                await Update_Cleanup_OK(latest);
            }
        }

        private static async Task Update_Stages_OK(ApplicationUpdate latest)
        {
            await latest.ExtractFiles(null);
            Assert.True(latest.UpdateStagingDirectory.Files.Count > 2);
        }

        private static async Task Update_Cleanup_OK(ApplicationUpdate latest)
        {
            await latest.CleanStaging(null);
            latest.UpdateFile.Delete();
            Assert.False(latest.UpdateFile.Exists);
            Assert.Empty(latest.UpdateStagingDirectory.Files);
        }

        [Fact]
        public void UpdateService_HasWritePermission_ReturnsExpected()
        {
            // Should reflect the underlying credential state
            var hasPermission = _updateService.HasWritePermission;
            Assert.IsType<bool>(hasPermission);
        }

        [Fact]
        public void UpdateService_UpdateCredential_ReturnsEnum()
        {
            var credential = _updateService.UpdateCredential;
            Assert.IsType<UpdateCredential>(credential);
        }

        [Fact]
        public void UpdateService_GetUpdateCredentials_ReturnsExpectedType()
        {
            var creds = _updateService.GetUpdateCredentials();
            // Should be null or WindowsImpersonation depending on credential
            Assert.True(creds == null || creds.GetType().Name == "WindowsImpersonation");
        }

        [Fact]
        public void UpdateService_Initialize_DoesNotThrow()
        {
            var service = new Mock_UpdateService();
            var exception = Record.Exception(() => service.Initialize());
            Assert.Null(exception);
        }

        // Example for RemoveIncompatibleReleases (indirectly)
        [Fact]
        public async Task UpdateService_AvailableUpdates_AreCompatible()
        {
            var service = new Mock_UpdateService();
            await service.GetUpdates();
            Assert.All(service.AvailableUpdates, update => Assert.True(update.PassesPrerequisiteChecks));
        }
    }
}
