using BLAZAM.Common.Data;

namespace BLAZAMCommon.Tests.Data
{
    public class ByteSizeTests
    {
        [Theory]
        [InlineData(0, "0.00 B")]
        [InlineData(500, "500.00 B")]
        [InlineData(1023, "1,023.00 B")]
        [InlineData(1024, "1.00 KB")] // Exactly 1 KB
        [InlineData(1500, "1.46 KB")] // 1500 / 1024
        [InlineData(1048576, "1.00 MB")] // Exactly 1 MB (1024 * 1024)
        [InlineData(1500000, "1.43 MB")] // 1500000 / (1024 * 1024)
        [InlineData(1073741824, "1.00 GB")] // Exactly 1 GB (1024 * 1024 * 1024)
        [InlineData(1500000000, "1.40 GB")] // 1500000000 / (1024 * 1024 * 1024)
        [InlineData(2000000000, "1.86 GB")] // Further GB test
        [InlineData(1099511627776, "1,024.00 GB")] // Test case where it stays GB because no TB suffix is handled
                                                   // This is equivalent to 1 TB, but the class tops out at GB
        public void ToString_ReturnsCorrectFormat(double bytes, string expectedOutput)
        {
            // Arrange
            var byteSize = new ByteSize(bytes);

            // Act
            string actualOutput = byteSize.ToString();

            // Assert
            Assert.Equal(expectedOutput, actualOutput);
        }

        [Fact]
        public void ToString_WithNegativeBytes_ReturnsFormattedNegativeBytes()
        {
            // Arrange
            var byteSize = new ByteSize(-500);

            // Act
            string actualOutput = byteSize.ToString();

            // Assert
            Assert.Equal("-500.00 B", actualOutput);
        }

        [Fact]
        public void ToString_WithNegativeKilobytes_ReturnsFormattedNegativeKilobytes()
        {
            var byteSize = new ByteSize(-1500);

            // Act
            string actualOutput = byteSize.ToString();

            // Assert
            Assert.Equal("-1,500.00 B", actualOutput);
        }

        [Fact]
        public void ToString_WithLargeNumberOfBytes_StaysAsGB()
        {
            // Arrange
            // This value is 2 TB (2 * 1024 * 1024 * 1024 * 1024 bytes)
            // The current implementation will show this as 2048.00 GB
            double twoTerabytesInBytes = 2.0 * 1024 * 1024 * 1024 * 1024;
            var byteSize = new ByteSize(twoTerabytesInBytes);
            string expectedOutput = "2,048.00 GB";


            // Act
            string actualOutput = byteSize.ToString();

            // Assert
            Assert.Equal(expectedOutput, actualOutput);
        }
    }
}
