﻿using BLAZAM.Server.Data.Services.Update;
using BLAZAM.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace BLAZAM.Tests.Updates
{
    public class UpdateTests
    {
        Mock_UpdateService _updateService = new Mock_UpdateService();
        [Fact]
        public async void Update_Returns_Data()
        {
            var latest = await _updateService.GetLatestUpdate();
            Assert.NotNull(latest);
        }
        [Fact]
        public async void Updat_Returns_ValidVersion()
        {
            var latest = await _updateService.GetLatestUpdate();

            Assert.NotNull(latest.Version);
        }
        [Fact]
        public async void Update_Returns_ValidDownload()
        {
            var latest = await _updateService.GetLatestUpdate();
            await latest.Download();

            Assert.True(latest.UpdateFile.Exists);
            Update_Stages_OK(latest);
            Update_Cleanup_OK(latest);
        }
        private async void Update_Stages_OK(ApplicationUpdate latest)
        {

            await latest.Stage();
            Assert.True(latest.UpdateStagingDirectory.Files.Count > 2);

        }
        private async void Update_Cleanup_OK(ApplicationUpdate latest)
        {

            latest.CleanStaging();
            latest.UpdateFile.Delete();
            Assert.True(!latest.UpdateFile.Exists);
            Assert.True(latest.UpdateStagingDirectory.Files.Count == 0);
        }
    }
}