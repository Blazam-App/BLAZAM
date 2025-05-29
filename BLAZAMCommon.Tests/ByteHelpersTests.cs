using System;
using Xunit;
using BLAZAM.Helpers; // Updated namespace based on the file content provided

namespace BLAZAM.Common.Tests
{
    public class ByteHelpersTests
    {
        // Tests for BitCount
        [Theory]
        [InlineData(0, 0)]
        [InlineData(255, 8)]
        [InlineData(170, 4)] // 10101010
        [InlineData(85, 4)]  // 01010101
        [InlineData(1, 1)]   // 00000001
        [InlineData(2, 1)]   // 00000010
        [InlineData(4, 1)]   // 00000100
        [InlineData(8, 1)]   // 00001000
        [InlineData(16, 1)]  // 00010000
        [InlineData(32, 1)]  // 00100000
        [InlineData(64, 1)]  // 01000000
        [InlineData(128, 1)] // 10000000
        public void BitCount_ShouldReturnCorrectCount(byte n, int expected)
        {
            Assert.Equal(expected, ByteHelpers.BitCount(n));
        }

        // Tests for BitDifference
        [Fact]
        public void BitDifference_IdenticalNonEmptyArrays_ShouldReturnZero()
        {
            var arr1 = new byte[] { 0b11110000, 0b00001111 };
            var arr2 = new byte[] { 0b11110000, 0b00001111 };
            Assert.Equal(0, arr1.BitDifference(arr2));
        }

        [Fact]
        public void BitDifference_DifferentArraysSameLength_ShouldReturnCorrectDifference()
        {
            var arr1 = new byte[] { 0b11110000 };
            var arr2 = new byte[] { 0b00001111 };
            Assert.Equal(8, arr1.BitDifference(arr2));
        }
        
        [Fact]
        public void BitDifference_SubSetArraySameLength_ShouldReturnCorrectDifference()
        {
            var arr1 = new byte[] { 0b10101010 };
            var arr2 = new byte[] { 0b00001010 };
            Assert.Equal(2, arr1.BitDifference(arr2));
        }

        [Fact]
        public void BitDifference_EmptyArrays_ShouldReturnZero()
        {
            var arr1 = Array.Empty<byte>();
            var arr2 = Array.Empty<byte>();
            Assert.Equal(0, arr1.BitDifference(arr2));
        }

        [Fact]
        public void BitDifference_OneNullArray_ShouldThrowArgumentNullException()
        {
            byte[]? arr1 = null;
            var arr2 = new byte[] { 0b11110000 };
            Assert.Throws<ArgumentNullException>(() => arr1.BitDifference(arr2));
            Assert.Throws<ArgumentNullException>(() => arr2.BitDifference(arr1));
        }

        [Fact]
        public void BitDifference_ArraysOfDifferentLengths_ShouldThrowArgumentException()
        {
            var arr1 = new byte[] { 0b11110000 };
            var arr2 = new byte[] { 0b00001111, 0b10101010 };
            Assert.Throws<ArgumentException>(() => arr1.BitDifference(arr2));
        }

        // Tests for ToByteArray
        [Fact]
        public void ToByteArray_Zero_ShouldReturnDefaultByteArray()
        {
            int number = 0;
            // After BitConverter.GetBytes and potential reverse, for 0, it's always {0,0,0,0} (BigEndian)
            var expected = new byte[] { 0, 0, 0, 0 };
            Assert.Equal(expected, number.ToByteArray());
        }

        [Fact]
        public void ToByteArray_PositiveInteger_ShouldReturnCorrectByteArray_BigEndian()
        {
            int number = 0x01020304; // 16909060
            // The method converts to BigEndian representation
            var expected = new byte[] { 1, 2, 3, 4 };
            Assert.Equal(expected, number.ToByteArray());
        }
        
        [Fact]
        public void ToByteArray_PositiveInteger_WithExplicitLength4_ShouldReturnCorrectByteArray_BigEndian()
        {
            int number = 0x01020304; // 16909060
            // The method converts to BigEndian representation
            var expected = new byte[] { 1, 2, 3, 4 };
            Assert.Equal(expected, number.ToByteArray(4));
        }

        [Fact]
        public void ToByteArray_NegativeInteger_ShouldReturnCorrectByteArray_BigEndian()
        {
            int number = -1; // 0xFFFFFFFF
            // The method converts to BigEndian representation
            var expected = new byte[] { 255, 255, 255, 255 };
            Assert.Equal(expected, number.ToByteArray());
        }

        [Fact]
        public void ToByteArray_IntMaxValue_ShouldReturnCorrectByteArray_BigEndian()
        {
            int number = int.MaxValue; // 0x7FFFFFFF
            // The method converts to BigEndian representation
            var expected = new byte[] { 127, 255, 255, 255 };
            Assert.Equal(expected, number.ToByteArray());
        }

        [Fact]
        public void ToByteArray_IntMinValue_ShouldReturnCorrectByteArray_BigEndian()
        {
            int number = int.MinValue; // 0x80000000
            // The method converts to BigEndian representation
            var expected = new byte[] { 128, 0, 0, 0 };
            Assert.Equal(expected, number.ToByteArray());
        }

        [Fact]
        public void ToByteArray_ShorterLength_ShouldTruncateAtEnd_BigEndian()
        {
            int number = 0x01020304; 
            // Initial BigEndian: {1, 2, 3, 4}
            // Array.Resize(ref byteArray, 2) truncates to {1, 2}
            var expected = new byte[] { 1, 2 };
            Assert.Equal(expected, number.ToByteArray(2));
        }

        [Fact]
        public void ToByteArray_LongerLength_ShouldPadWithZerosAtEnd_BigEndian()
        {
            int number = 0x01020304; 
            // Initial BigEndian: {1, 2, 3, 4}
            // Array.Resize(ref byteArray, 6) pads to {1, 2, 3, 4, 0, 0}
            var expected = new byte[] { 1, 2, 3, 4, 0, 0 };
            Assert.Equal(expected, number.ToByteArray(6));
        }
    }
}
