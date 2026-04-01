namespace BLAZAM.FileSystem
{
    /// <summary>
    /// Represents the progress of a file operation, typically download or upload.
    /// </summary>
    public class FileProgress
    {
        /// <summary>
        /// Gets the progress of the file operation as a percentage (0-100).
        /// </summary>
        public int FilePercentage => ExpectedSize == 0 ? 0 : (int)((double)CompletedBytes / ExpectedSize * 100);

        /// <summary>
        /// Gets or sets the total expected size of the file in bytes.
        /// </summary>
        public long ExpectedSize { get; set; }   // Changed to long

        /// <summary>
        /// Gets or sets the number of bytes completed in the file operation.
        /// </summary>
        public long CompletedBytes { get; set; } // Changed to long
    }
}
