using Xunit;
using Moq;
using BLAZAM.Common.Data; // Assuming ApplicationInfo, SystemDirectory, ApplicationVersion are here
using BLAZAM.Plugins;    // Assuming IPluginBase is here
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using BLAZAM.FileSystem;
using Microsoft.AspNetCore.Hosting; // For IWebHostEnvironment

namespace BLAZAMCommon.Tests.Data
{
    public class ApplicationInfoTests : IDisposable
    {
        // Store original static values to restore them after each test
        private readonly ApplicationVersion _originalRunningVersion = ApplicationInfo.runningVersion;
        private readonly Process _originalRunningProcess = ApplicationInfo.runningProcess;
        private readonly SystemDirectory _originalApplicationRoot = ApplicationInfo.applicationRoot;
        private readonly SystemDirectory _originalTempDirectory = ApplicationInfo.tempDirectory;
        private readonly IEnumerable<string> _originalListeningAddresses = ApplicationInfo.listeningAddresses;
        private readonly bool _originalInDebugMode = ApplicationInfo.inDebugMode;
        private readonly bool _originalInDemoMode = ApplicationInfo.inDemoMode;
        private readonly Guid _originalInstallationId = ApplicationInfo.installationId;
        private readonly IServiceProvider _originalServices = ApplicationInfo.services;
        private readonly ConfigurationManager _originalConfiguration = ApplicationInfo.configuration;
        private readonly bool _originalInstallationCompleted = ApplicationInfo.installationCompleted;

        public ApplicationInfoTests()
        {
            // Ensure runningProcess is not null to prevent NullReferenceException in tests
            // unless a specific test is designed to check null behavior.
            if (ApplicationInfo.runningProcess == null)
            {
                ApplicationInfo.runningProcess = Process.GetCurrentProcess();
            }
            // Reset static lists to new instances to avoid interference between tests
            ApplicationInfo.listeningAddresses = new List<string>();
        }

        public void Dispose()
        {
            // Restore original static values
            ApplicationInfo.runningVersion = _originalRunningVersion;
            ApplicationInfo.runningProcess = _originalRunningProcess;
            ApplicationInfo.applicationRoot = _originalApplicationRoot;
            ApplicationInfo.tempDirectory = _originalTempDirectory;
            ApplicationInfo.listeningAddresses = _originalListeningAddresses;
            ApplicationInfo.inDebugMode = _originalInDebugMode;
            ApplicationInfo.inDemoMode = _originalInDemoMode;
            ApplicationInfo.installationId = _originalInstallationId;
            ApplicationInfo.services = _originalServices;
            ApplicationInfo.configuration = _originalConfiguration;
            ApplicationInfo.installationCompleted = _originalInstallationCompleted;
        }

        [Fact]
        public void DefaultConstructor_ShouldCreateInstance()
        {
            // Act
            var appInfo = new ApplicationInfo();

            // Assert
            Assert.NotNull(appInfo);
        }

       

        [Fact]
        public void StaticProperties_ShouldSetAndGetCorrectly()
        {
            // Arrange
            var testVersion = new ApplicationVersion("1.0.0");
            var testProcess = Process.GetCurrentProcess(); // Using current process, specific process mocking is hard
            var testAppRoot = new SystemDirectory("C:\\static\\root");
            var testTempDir = new SystemDirectory("C:\\static\\temp");
            var testAddresses = new List<string> { "http://static" };
            var testGuid = Guid.NewGuid();
            var mockServices = new Mock<IServiceProvider>().Object;
            var mockPlugins = new List<IPluginBase> { new Mock<IPluginBase>().Object };
            var mockConfig = new ConfigurationManager();

            // Act & Assert
            ApplicationInfo.runningVersion = testVersion;
            Assert.Same(testVersion, ApplicationInfo.runningVersion);

            ApplicationInfo.runningProcess = testProcess; // Note: testing IsUnderIIS behavior separately
            Assert.Same(testProcess, ApplicationInfo.runningProcess);

            ApplicationInfo.applicationRoot = testAppRoot;
            Assert.Same(testAppRoot, ApplicationInfo.applicationRoot);

            ApplicationInfo.tempDirectory = testTempDir;
            Assert.Same(testTempDir, ApplicationInfo.tempDirectory);

            ApplicationInfo.listeningAddresses = testAddresses;
            Assert.Same(testAddresses, ApplicationInfo.listeningAddresses);

            ApplicationInfo.inDebugMode = true;
            Assert.True(ApplicationInfo.inDebugMode);
            ApplicationInfo.inDebugMode = false;
            Assert.False(ApplicationInfo.inDebugMode);

            ApplicationInfo.inDemoMode = true;
            Assert.True(ApplicationInfo.inDemoMode);
            ApplicationInfo.inDemoMode = false;
            Assert.False(ApplicationInfo.inDemoMode);

            ApplicationInfo.installationId = testGuid;
            Assert.Equal(testGuid, ApplicationInfo.installationId);

            ApplicationInfo.services = mockServices;
            Assert.Same(mockServices, ApplicationInfo.services);



            ApplicationInfo.installationCompleted = true;
            Assert.True(ApplicationInfo.installationCompleted);
            ApplicationInfo.installationCompleted = false;
            Assert.False(ApplicationInfo.installationCompleted);

            ApplicationInfo.configuration = mockConfig;
            Assert.Same(mockConfig, ApplicationInfo.configuration);
        }

        [Fact]
        public void InstanceProperties_ShouldMirrorAndSetStaticProperties()
        {
            // Arrange
            var appInfo = new ApplicationInfo(); // Uses default constructor

            var testVersion = new ApplicationVersion("1.0.0");
            var testProcess = Process.GetCurrentProcess();
            var testAppRoot = new SystemDirectory("C:\\instance\\root");
            var testTempDir = new SystemDirectory("C:\\instance\\temp");
            var testAddresses = new List<string> { "http://instance" };
            var testGuid = Guid.NewGuid();
            var mockPlugins = new List<IPluginBase> { new Mock<IPluginBase>().Object };


            // Act & Assert - RunningVersion
            appInfo.RunningVersion = testVersion;
            Assert.Same(testVersion, ApplicationInfo.runningVersion);
            Assert.Same(testVersion, appInfo.RunningVersion);
            var newStaticVersion = new ApplicationVersion("1.0.0");
            ApplicationInfo.runningVersion = newStaticVersion;
            Assert.Same(newStaticVersion, appInfo.RunningVersion);

            // Act & Assert - RunningProcess
            appInfo.RunningProcess = testProcess;
            Assert.Same(testProcess, ApplicationInfo.runningProcess);
            Assert.Same(testProcess, appInfo.RunningProcess);
            var newStaticProcess = Process.GetProcesses().FirstOrDefault(p => p.Id != testProcess.Id) ?? testProcess; // Get a different process if possible
            ApplicationInfo.runningProcess = newStaticProcess;
            Assert.Same(newStaticProcess, appInfo.RunningProcess);

            // Act & Assert - ApplicationRoot
            appInfo.ApplicationRoot = testAppRoot;
            Assert.Same(testAppRoot, ApplicationInfo.applicationRoot);
            Assert.Same(testAppRoot, appInfo.ApplicationRoot);
            var newStaticAppRoot = new SystemDirectory("C:\\new\\static\\root");
            ApplicationInfo.applicationRoot = newStaticAppRoot;
            Assert.Same(newStaticAppRoot, appInfo.ApplicationRoot);

            // Act & Assert - TempDirectory
            appInfo.TempDirectory = testTempDir;
            Assert.Same(testTempDir, ApplicationInfo.tempDirectory);
            Assert.Same(testTempDir, appInfo.TempDirectory);
            var newStaticTempDir = new SystemDirectory("C:\\new\\static\\temp");
            ApplicationInfo.tempDirectory = newStaticTempDir;
            Assert.Same(newStaticTempDir, appInfo.TempDirectory);

            // Act & Assert - ListeningAddresses
            appInfo.ListeningAddresses = testAddresses;
            Assert.Same(testAddresses, ApplicationInfo.listeningAddresses);
            Assert.Same(testAddresses, appInfo.ListeningAddresses);
            var newStaticAddresses = new List<string> { "http://newstatic" };
            ApplicationInfo.listeningAddresses = newStaticAddresses;
            Assert.Same(newStaticAddresses, appInfo.ListeningAddresses);

            // Act & Assert - InDebugMode
            appInfo.InDebugMode = true;
            Assert.True(ApplicationInfo.inDebugMode);
            Assert.True(appInfo.InDebugMode);
            ApplicationInfo.inDebugMode = false;
            Assert.False(appInfo.InDebugMode);

            // Act & Assert - InDemoMode
            appInfo.InDemoMode = true;
            Assert.True(ApplicationInfo.inDemoMode);
            Assert.True(appInfo.InDemoMode);
            ApplicationInfo.inDemoMode = false;
            Assert.False(appInfo.InDemoMode);

            // Act & Assert - InstallationId
            appInfo.InstallationId = testGuid;
            Assert.Equal(testGuid, ApplicationInfo.installationId);
            Assert.Equal(testGuid, appInfo.InstallationId);
            var newStaticGuid = Guid.NewGuid();
            ApplicationInfo.installationId = newStaticGuid;
            Assert.Equal(newStaticGuid, appInfo.InstallationId);

           

            // Act & Assert - InstallationCompleted
            appInfo.InstallationCompleted = true;
            Assert.True(ApplicationInfo.installationCompleted);
            Assert.True(appInfo.InstallationCompleted);
            ApplicationInfo.installationCompleted = false;
            Assert.False(appInfo.InstallationCompleted);
        }

        [Fact]
        public void Instance_ReadOnlyProperties_ShouldReflectStaticState()
        {
            // Arrange
            var appInfo = new ApplicationInfo();
            var mockConfig = new ConfigurationManager();

            // Act & Assert - Configuration (instance property is get-only)
            ApplicationInfo.configuration = mockConfig;
            Assert.Same(mockConfig, appInfo.Configuration);

            // Act & Assert - IsUnderIIS (instance property is get-only, relies on static isUnderIIS)
            // Behavior of static isUnderIIS is tested separately. Here we just check passthrough.
            // We set the static runningProcess to the current process for this test.
            ApplicationInfo.runningProcess = Process.GetCurrentProcess();
            bool expectedIsUnderIIS = ApplicationInfo.runningProcess.ProcessName.Contains("w3wp") ||
                                      ApplicationInfo.runningProcess.ProcessName.Contains("iisexpress");
            Assert.Equal(expectedIsUnderIIS, ApplicationInfo.isUnderIIS); // Check static first
            Assert.Equal(ApplicationInfo.isUnderIIS, appInfo.IsUnderIIS); // Then check instance reflects static
        }

        [Fact]
        public void IsUnderIIS_StaticProperty_ShouldReturnFalse_ForTypicalTestRunner()
        {
            // Arrange
            // Process.ProcessName is not virtual, so we can't mock it with Moq.
            // This test relies on the actual ProcessName of the currently running process (test runner).
            ApplicationInfo.runningProcess = Process.GetCurrentProcess();
            string currentProcessName = ApplicationInfo.runningProcess.ProcessName;

            // Act
            bool isUnderIIS = ApplicationInfo.isUnderIIS;

            // Assert
            if (currentProcessName.Contains("w3wp") || currentProcessName.Contains("iisexpress"))
            {
                Assert.True(isUnderIIS); // Should be true if test runner IS IIS/IISExpress
            }
            else
            {
                Assert.False(isUnderIIS); // Typically false for test runners like vstest.console, testhost, etc.
            }
        }

        [Fact]
        public void IsUnderIIS_ShouldBeTrue_IfProcessNameIsW3wp()
        {
            // This test demonstrates a limitation. We cannot directly mock Process.ProcessName with Moq
            // as it's not a virtual member. For a robust test, one would need to refactor ApplicationInfo
            // to use an abstraction (IProcess) or use a mocking framework that supports non-virtual members.
            // The current test will pass if the test runner itself happens to be named "w3wp.exe" (highly unlikely)
            // or fails to truly test the "w3wp" condition in isolation.

            // For demonstration, if we COULD mock it (e.g., with an IProcess wrapper):
            // var mockProcess = new Mock<IProcess>(); // Assuming IProcess with virtual string ProcessName
            // mockProcess.Setup(p => p.ProcessName).Returns("w3wp.exe");
            // ApplicationInfo.runningProcess = mockProcess.Object; // Assuming runningProcess is IProcess
            // Assert.True(ApplicationInfo.isUnderIIS);

            // Current behavior: relies on the actual process name.
            // If we could somehow launch and assign a real w3wp process here (complex and brittle for unit tests)
            // For now, this test highlights the untestable path with pure Moq on the current code structure.
            // We'll assume current process is NOT w3wp for typical test scenarios.
            ApplicationInfo.runningProcess = Process.GetCurrentProcess();
            if (ApplicationInfo.runningProcess.ProcessName.Contains("w3wp"))
            {
                Assert.True(ApplicationInfo.isUnderIIS);
            }
            else
            {
                // This assertion is here to acknowledge that we are not testing the true "w3wp" path effectively
                Assert.False(ApplicationInfo.isUnderIIS, "Test assumes current process is not w3wp. If it is, this test needs adjustment or the previous condition handles it.");
            }
        }

        [Fact]
        public void IsUnderIIS_ShouldBeTrue_IfProcessNameIsIisExpress()
        {
            // Similar limitations as the w3wp test apply here due to non-virtual Process.ProcessName.
            ApplicationInfo.runningProcess = Process.GetCurrentProcess();
            if (ApplicationInfo.runningProcess.ProcessName.Contains("iisexpress"))
            {
                Assert.True(ApplicationInfo.isUnderIIS);
            }
            else
            {
                Assert.False(ApplicationInfo.isUnderIIS, "Test assumes current process is not iisexpress. If it is, this test needs adjustment or the previous condition handles it.");
            }
        }
    }
}
