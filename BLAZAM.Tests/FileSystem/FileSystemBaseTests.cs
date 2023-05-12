using BLAZAM.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Tests.FileSystem
{
    public class FileSystemBaseTests
    {
        [Fact]
        public void Constructor_ThrowsArgumentException_WhenPathIsNull()
        {
            // Arrange
            string path = null;

            // Act and Assert
            Assert.Throws<ArgumentException>(() => new FileSystemBase(path));
        }

        [Fact]
        public void Constructor_ReplacesTempVariable_WhenPathContainsTemp()
        {
            // Arrange
            string path = "%temp%\\test.txt";

            // Act
            var fileSystemBase = new FileSystemBase(path);

            // Assert
            Assert.Equal(Path.GetTempPath() + "test.txt", fileSystemBase.Path);
        }

        [Fact]
        public void Constructor_SetsFullPath_WhenPathIsRelative()
        {
            // Arrange
            string path = "..\\test.txt";

            // Act
            var fileSystemBase = new FileSystemBase(path);

            // Assert
            Assert.Equal(Path.GetFullPath(path), fileSystemBase.Path);
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
        //TODO Fix checking when no write permission
        //[Fact]
        //public void Writable_ReturnsFalse_WhenFileHasNoWritePermission()
        //{
        //    // Arrange
        //    string path = Path.GetTempFileName();
        //    var fileSystemBase = new FileSystemBase(path);

        //    // Deny write permission to the file
        //    var ac = new FileInfo(path).GetAccessControl();
        //    ac.AddAccessRule(new FileSystemAccessRule(Environment.UserName, FileSystemRights.Write, AccessControlType.Deny));
        //    new FileInfo(path).SetAccessControl(ac);

        //    // Act
        //    bool writable = fileSystemBase.Writable;

        //    // Assert
        //    Assert.False(writable);

        //    // Clean up
        //    File.Delete(path);
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
            string toString = fileSystemBase.ToString();

            // Assert
            Assert.Equal(path, toString);

            // Clean up
            File.Delete(path);
        }
    }

}
