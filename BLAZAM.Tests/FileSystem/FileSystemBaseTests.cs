using BLAZAM.FileSystem;

namespace BLAZAM.Tests.FileSystem
{
    public class FileSystemTests
    {
        [Fact]
        public void Constructor_ThrowsArgumentException_WhenPathIsNull()
        {
            // Arrange
            string? path = null;

            // Act and Assert
#pragma warning disable CS8604 // Possible null reference argument.
            Assert.Throws<ArgumentNullException>(() => new FileSystemBase(path));
#pragma warning restore CS8604 // Possible null reference argument.
        }

        [Fact]
        public void Constructor_ReplacesTempVariable_WhenPathContainsTemp()
        {
            // Arrange
            string path = "%temp%" + Path.DirectorySeparatorChar + "test.txt";

            // Act
            var fileSystemBase = new FileSystemBase(path);

            // Assert
            Assert.Equal(Path.GetTempPath() + "test.txt", fileSystemBase.FullPath);
        }

        [Fact]
        public void Constructor_SetsFullPath_WhenPathIsRelative()
        {
            // Arrange
            string path = "..\\test.txt";

            // Act
            var fileSystemBase = new FileSystemBase(path);

            // Assert
            Assert.Equal(Path.GetFullPath(path), fileSystemBase.FullPath);
        }

        [Fact]
        public void Writable_ReturnsTrue_WhenFileHasWritePermission()
        {
            // Arrange
            string path = Path.GetTempFileName();
            var fileSystemBase = new FileSystemBase(path);

            // Act
            bool writable = fileSystemBase.Writable;

            // Assert
            Assert.True(writable);

            // Clean up
            File.Delete(path);
        }

        //[Fact]
        //public void Writable_ReturnsFalse_WhenFileHasNoWritePermission()
        //{
        //    // Arrange
        //    string? tempFile = null;
        //    try
        //    {
        //        tempFile = Path.GetTempFileName();
        //        File.SetAttributes(tempFile, FileAttributes.ReadOnly);
        //        var fileSystemBase = new FileSystemBase(tempFile);

        //        // Act
        //        bool writable = fileSystemBase.Writable;

        //        // Assert
        //        Assert.False(writable);
        //    }
        //    finally
        //    {
        //        // Clean up
        //        if (tempFile != null && File.Exists(tempFile))
        //        {
        //            File.SetAttributes(tempFile, FileAttributes.Normal); // Remove read-only attribute
        //            File.Delete(tempFile);
        //        }
        //    }
        //}

        [Fact]
        public void Writable_ReturnsTrue_WhenDirHasWritePermission()
        {
            // Arrange
            string path = Path.GetTempFileName();
            var tmpDir = Path.GetDirectoryName(path);
            if (tmpDir == null)
            {
                throw new InvalidOperationException("Temporary directory path is null.");
            }
            var fileSystemBase = new FileSystemBase(tmpDir);


            // Act
            bool writable = fileSystemBase.Writable;

            // Assert
            Assert.True(writable);

            // Clean up
            File.Delete(path);
        }

        //[Fact]
        //public void Writable_ReturnsFalse_WhenDirHasNoWritePermission()
        //{
        //    // Arrange
        //    // Using a system directory that is typically not writable by a non-admin user.
        //    // This makes the test somewhat environment-dependent but avoids needing to
        //    // execute external processes like icacls or chmod.
        //    string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        //    // If ProgramFiles is empty (unlikely but possible in some stripped environments),
        //    // or if for some reason it's writable (e.g. tests running as admin), this test might not be meaningful.
        //    // However, for typical scenarios, it should be non-writable.
        //    if (string.IsNullOrEmpty(programFilesPath) || !Directory.Exists(programFilesPath))
        //    {
        //        // Fallback or skip if the directory doesn't exist.
        //        // For this test, we'll assume it exists if we reach here.
        //        // An alternative would be Assert.True(false, "Program Files directory not found, cannot run test.")
        //        // but that might be too strict. Let's proceed assuming it exists.
        //        // If it doesn't exist, FileSystemBase constructor might throw, or Writable might be false for other reasons.
        //    }

        //    var fileSystemBase = new FileSystemBase(programFilesPath);

        //    // Act
        //    bool writable = fileSystemBase.Writable;

        //    // Assert
        //    // This assertion depends on the FileSystemBase.Writable implementation for directories.
        //    // It often checks if a small file can be created in the directory.
        //    // If running as Admin, Program Files might be writable, and this test would fail.
        //    // This is an inherent challenge with testing "not writable" directory permissions
        //    // without more control over the environment or the FileSystemBase internals.
        //    Assert.False(writable);
        //}

        [Fact]
        public void GetHashCode_ReturnsPathHashCode()
        {
            // Arrange
            string path = Path.GetTempFileName();
            var fileSystemBase = new FileSystemBase(path);

            // Act
            int hashCode = fileSystemBase.GetHashCode();

            // Assert
            Assert.Equal(path.GetHashCode(), hashCode);

            // Clean up
            File.Delete(path);
        }

        [Fact]
        public void ToString_ReturnsPath()
        {
            // Arrange
            string path = Path.GetTempFileName();
            var fileSystemBase = new FileSystemBase(path);

            // Act
            string? toString = fileSystemBase.ToString();

            // Assert
            Assert.Equal(path, toString);

            // Clean up
            File.Delete(path);
        }

        [Fact]
        public void FilePercentage_WhenExpectedSizeIsZero_ReturnsZero()
        {
            // Arrange
            var fileProgress = new FileProgress
            {
                ExpectedSize = 0,
                CompletedBytes = 100
            };

            // Act
            var percentage = fileProgress.FilePercentage;

            // Assert
            Assert.Equal(0, percentage);
        }

        [Fact]
        public void FilePercentage_WhenNoProgress_ReturnsZero()
        {
            // Arrange
            var fileProgress = new FileProgress
            {
                ExpectedSize = 1000,
                CompletedBytes = 0
            };

            // Act
            var percentage = fileProgress.FilePercentage;

            // Assert
            Assert.Equal(0, percentage);
        }

        [Fact]
        public void FilePercentage_WhenHalfComplete_ReturnsFiftyPercent()
        {
            // Arrange
            var fileProgress = new FileProgress
            {
                ExpectedSize = 1000,
                CompletedBytes = 500
            };

            // Act
            var percentage = fileProgress.FilePercentage;

            // Assert
            Assert.Equal(50, percentage);
        }

        [Fact]
        public void FilePercentage_WhenFullyComplete_ReturnsOneHundredPercent()
        {
            // Arrange
            var fileProgress = new FileProgress
            {
                ExpectedSize = 1000,
                CompletedBytes = 1000
            };

            // Act
            var percentage = fileProgress.FilePercentage;

            // Assert
            Assert.Equal(100, percentage);
        }

        [Fact]
        public void FilePercentage_WithLargeFileSize_CalculatesCorrectly()
        {
            // Arrange
            var fileProgress = new FileProgress
            {
                ExpectedSize = 5_000_000_000, // 5 GB
                CompletedBytes = 1_250_000_000 // 1.25 GB
            };

            // Act
            var percentage = fileProgress.FilePercentage;

            // Assert
            Assert.Equal(25, percentage);
        }

        [Fact]
        public void FilePercentage_WithSmallProgress_RoundsDown()
        {
            // Arrange
            var fileProgress = new FileProgress
            {
                ExpectedSize = 1000,
                CompletedBytes = 1
            };

            // Act
            var percentage = fileProgress.FilePercentage;

            // Assert
            Assert.Equal(0, percentage); // 0.1% rounds down to 0
        }

        [Theory]
        [InlineData(1000, 250, 25)]
        [InlineData(1000, 750, 75)]
        [InlineData(100, 33, 33)]
        [InlineData(100, 99, 99)]
        [InlineData(10000, 9999, 99)]
        public void FilePercentage_WithVariousValues_CalculatesCorrectly(long expectedSize, long completedBytes, int expectedPercentage)
        {
            // Arrange
            var fileProgress = new FileProgress
            {
                ExpectedSize = expectedSize,
                CompletedBytes = completedBytes
            };

            // Act
            var percentage = fileProgress.FilePercentage;

            // Assert
            Assert.Equal(expectedPercentage, percentage);
        }

        [Fact]
        public void ExpectedSize_CanBeSet()
        {
            // Arrange
            var fileProgress = new FileProgress();

            // Act
            fileProgress.ExpectedSize = 2048;

            // Assert
            Assert.Equal(2048, fileProgress.ExpectedSize);
        }

        [Fact]
        public void CompletedBytes_CanBeSet()
        {
            // Arrange
            var fileProgress = new FileProgress();

            // Act
            fileProgress.CompletedBytes = 1024;

            // Assert
            Assert.Equal(1024, fileProgress.CompletedBytes);
        }
    }

}
