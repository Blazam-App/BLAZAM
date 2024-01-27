namespace BLAZAM.Helpers
{
    public static class ByteHelpers
    {

        // A method that returns the number of 1s in a byte using Brian Kernighan's algorithm
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


        // A method that returns the number of different bits between two byte arrays
        public static int BitDifference(this byte[] a, byte[] b)
        {
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