using BLAZAM.Logger; // Added

namespace BLAZAM.FileSystem
{
    /// <summary>
    /// Represents a file in the file system, providing properties and methods for file manipulation and access.
    /// </summary>
    public class SystemFile : FileSystemBase, IFileSystemObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemFile"/> class.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        public SystemFile(string path) : base(path)
        {
        }

        /// <summary>
        /// Gets a value indicating whether the file currently exists.
        /// </summary>
        public bool Exists => File.Exists(FullPath);

        /// <summary>
        /// Gets the name of the file without the extension.
        /// </summary>
        public override string Name => Path.GetFileNameWithoutExtension(FullPath);

        /// <summary>
        /// Gets the file extension.
        /// </summary>
        public string Extension => Path.GetExtension(FullPath);

        /// <summary>
        /// Gets the parent directory of this file. Returns a representation of the current directory if the parent cannot be determined.
        /// </summary>
        public SystemDirectory ParentDirectory
        {
            get
            {
                string? directoryName = Path.GetDirectoryName(FullPath);
                if (string.IsNullOrEmpty(directoryName))
                {
                    Loggers.SystemLogger.Warning("SystemFile.ParentDirectory: Could not determine directory name for {FullPath}. Returning current directory representation (\".\").", FullPath);
                    return new SystemDirectory(".");
                }
                return new SystemDirectory(directoryName);
            }
        }

        /// <summary>
        /// Asynchronously reads the contents of the file as a byte array. Returns an empty array on error.
        /// </summary>
        /// <returns>A byte array containing the file's contents, or an empty array if an error occurs.</returns>
        public async Task<byte[]> ReadAllBytesAsync()
        {
            try
            {
                return await File.ReadAllBytesAsync(FullPath);
            }
            catch (Exception ex) // Catches IOException, SecurityException, etc.
            {
                Loggers.SystemLogger.Error(ex, "SystemFile.ReadAllBytesAsync: Error reading all bytes async from {FilePath}.", FullPath);
                return Array.Empty<byte>();
            }
        }

        /// <summary>
        /// Reads the contents of the file as a byte array. Returns an empty array on error.
        /// </summary>
        /// <returns>A byte array containing the file's contents, or an empty array if an error occurs.</returns>
        public byte[] ReadAllBytes()
        {
            try
            {
                return File.ReadAllBytes(FullPath);
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "SystemFile.ReadAllBytes: Error reading all bytes from {FilePath}.", FullPath);
                return Array.Empty<byte>();
            }
        }

        /// <summary>
        /// Reads the contents of the file as a string. Returns an empty string on error.
        /// </summary>
        /// <returns>A string containing the file's contents, or an empty string if an error occurs.</returns>
        public string ReadAllText()
        {
            try
            {
                return File.ReadAllText(FullPath);
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "SystemFile.ReadAllText: Error reading all text from {FilePath}.", FullPath);
                return string.Empty;
            }
        }

        /// <summary>
        /// Writes the given text to the file, overwriting any existing content. Creates the file if it does not exist.
        /// </summary>
        /// <param name="text">The text to write. If null, an empty string will be written.</param>
        /// <returns>True if successful, false if an error occurs.</returns>
        public bool WriteAllText(string? text)
        {
            if (text == null)
            {
                Loggers.SystemLogger.Debug("SystemFile.WriteAllText: Input text is null for file {FilePath}. Writing empty string.", FullPath);
                text = string.Empty;
            }
            try
            {
                File.WriteAllText(FullPath, text);
                return true;
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "SystemFile.WriteAllText: Error writing all text to {FilePath}.", FullPath);
                return false;
            }
        }

        /// <summary>
        /// Gets the date and time the file was last modified.
        /// </summary>
        public DateTime LastModified { get => File.GetLastWriteTime(FullPath); }

        /// <summary>
        /// Gets the time elapsed since the file was last modified.
        /// </summary>
        public TimeSpan SinceLastModified { get => DateTime.Now - LastModified; }

        /// <summary>
        /// Gets a value indicating whether the current user has write permissions.
        /// If the file exists, it checks write permission on the file itself.
        /// If the file does not exist, it checks write permission on the parent directory.
        /// </summary>
        public override bool Writable
        {
            get
            {
                if (this.Exists) return base.Writable; // base.Writable checks the file itself
                return ParentDirectory.Writable; // Checks parent directory if file doesn't exist
            }
        }

        /// <summary>
        /// Deletes the file. Logs an error if the operation fails but does not throw.
        /// </summary>
        public void Delete()
        {
            try
            {
                File.Delete(FullPath);
            }
            catch (Exception ex) // Catches IOException, UnauthorizedAccessException, etc.
            {
                Loggers.SystemLogger.Error(ex, "SystemFile.Delete: Error deleting file {FilePath}.", FullPath);
            }
        }

        /// <summary>
        /// Opens the file for reading. Returns null on error.
        /// </summary>
        /// <returns>A FileStream for reading, or null if an error occurs.</returns>
        public FileStream? OpenReadStream()
        {
            try
            {
                return new FileStream(FullPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true); // Changed FileShare.None to FileShare.Read
            }
            catch (Exception ex) // Catches FileNotFoundException, UnauthorizedAccessException, IOException, etc.
            {
                Loggers.SystemLogger.Debug(ex, "SystemFile.OpenReadStream: Error opening read stream for {FilePath}.", FullPath);
                return null;
            }
        }

        /// <summary>
        /// Opens or creates the file for writing. Returns null on error.
        /// </summary>
        /// <returns>A FileStream for writing, or null if an error occurs.</returns>
        public FileStream? OpenWriteStream()
        {
            try
            {
                // FileMode.Create will overwrite if exists, or create if not.
                return new FileStream(FullPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
            }
            catch (Exception ex) // Catches UnauthorizedAccessException, IOException, etc.
            {
                Loggers.SystemLogger.Error(ex, "SystemFile.OpenWriteStream: Error opening write stream for {FilePath}.", FullPath);
                return null;
            }
        }



        /// <summary>
        /// Creates this file as an empty file if it does not already exist.
        /// Ensures the parent directory exists before creating the file.
        /// </summary>
        public bool Create()
        {
            try
            {
                if (!ParentDirectory.Exists)
                {
                    // ParentDirectory.EnsureCreated() already has its own logging
                    ParentDirectory.Create();
                }
            }
            catch (Exception ex) // Catching exception from ParentDirectory.EnsureCreated if it throws despite internal logging
            {
                Loggers.SystemLogger.Error(ex, "SystemFile.Create: Error ensuring parent directory exists for {FilePath} during Create.", FullPath);
                // Depending on requirements, might want to return or re-throw here.
                // For now, it will proceed to attempt FileStream creation, which will likely also fail.
            }

            FileStream? stream = null;
            try
            {
                // Using FileMode.OpenOrCreate to avoid overwriting existing files
                stream = new FileStream(FullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, bufferSize: 4096, useAsync: true);
                return true;
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "SystemFile.Create: Error creating file stream for {FilePath} during Create.", FullPath);
                // Depending on requirements, might want to re-throw.
            }
            finally
            {
                stream?.Close(); // Ensure stream is closed if it was opened
            }

            return false;
        }

        public override bool Rename(string newName)
        {
            try
            {
                var newPath = Path.Combine(ParentDirectory.FullPath, newName);
                File.Move(FullPath, newPath);
                FullPath = newPath;
                return true;
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Error renaming file {File} to {NewName}", FullPath, newName);
                return false;
            }
        }

        public bool CopyTo(SystemDirectory parentDirectory)
        {
            try
            {
                if (!parentDirectory.Exists)
                {
                    parentDirectory.Create();
                }
                var destinationPath = Path.Combine(parentDirectory.FullPath, this.Name + this.Extension);
                File.Copy(this.FullPath, destinationPath, overwrite: false);
                return true;
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Information(ex, "Error copying file {File} to directory {Directory}", FullPath, parentDirectory.FullPath);
                return false;
            }
        }
    }
}