namespace BLAZAM.Helpers
{
    /// <summary>
    /// Provides extension methods and utilities for byte manipulation and conversions.
    /// </summary>
    public static class ByteHelpers
    {
        /// <summary>
        /// Counts the number of set bits (1s) in a byte using Brian Kernighan's algorithm.
        /// </summary>
        /// <param name="n">The byte to count bits in.</param>
        /// <returns>The number of set bits in the byte.</returns>
        public static int BitCount(this byte n)
        {
            // Initialize a counter for 1s
            int count = 0;

            // Loop until n becomes zero
            while (n > 0)
            {
                // Clear the least significant bit set to 1 and increment the counter
                n &= (byte)(n - 1);
                count++;
            }

            // Return the number of 1s in n
            return count;
        }

        /// <summary>
        /// Calculates the number of differing bits between two byte arrays of equal length.
        /// </summary>
        /// <param name="a">The first byte array.</param>
        /// <param name="b">The second byte array.</param>
        /// <returns>The total number of differing bits.</returns>
        /// <exception cref="ArgumentNullException">Thrown if either input array is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the input arrays do not have the same length.</exception>
        public static int BitDifference(this byte[]? a, byte[]? b)
        {
            ArgumentNullException.ThrowIfNull(a);
            ArgumentNullException.ThrowIfNull(b);


            // Check that the arrays have the same length
            if (a.Length != b.Length) throw new ArgumentException("Arrays must have the same length");

            // Initialize a counter for different bits
            int diff = 0;

            // Loop through each byte in the arrays
            for (int i = 0; i < a.Length; i++)
            {
                // XOR the bytes and count the number of 1s in the result
                diff += ((byte)(a[i] ^ b[i])).BitCount();
            }

            // Return the total number of different bits
            return diff;
        }

        /// <summary>
        /// Converts an integer to a byte array, optionally padding with leading zeros to a specified length.
        /// </summary>
        /// <param name="number">The integer to convert.</param>
        /// <param name="length">Optional. The desired length of the byte array. If specified and greater than the natural length, the array will be padded with leading zeros (or truncated if shorter, though current implementation only pads).</param>
        /// <returns>A byte array representing the integer.</returns>
        public static byte[] ToByteArray(this int number, int? length = null)
        {
            byte[] byteArray = BitConverter.GetBytes(number);

            // Check endianness and reverse byte array if necessary
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(byteArray);
            }
            if (length != null)
                // Pad the byte array to the desired length with zeroes
                Array.Resize(ref byteArray, (int)length);

            return byteArray;
        }
    }
}