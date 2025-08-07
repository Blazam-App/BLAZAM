using BLAZAM.Logger;
using Serilog;
using System.Security;

namespace BLAZAM.FileSystem
{
    /// <summary>
    /// Base class for representing entries in the file system, such as files or directories.
    /// It provides common properties like the full path and methods to check writability.
    /// </summary>
    public class FileSystemBase : IEquatable<FileSystemBase?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemBase"/> class with the specified path.
        /// Converts environment variables like "%temp%" to their full paths.
        /// </summary>
        /// <param name="path">The initial path to the file system entry. Environment variables like "%temp%" are expanded.</param>
        /// <exception cref="ArgumentNullException">Thrown if path is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the path is invalid or cannot be resolved to a full path.</exception>
        public FileSystemBase(string path)
        {
            ArgumentNullException.ThrowIfNull(path);

            string originalPath = path; // Store original path for logging
            path = path.Replace("%temp%", Path.GetTempPath());

            try
            {
                FullPath = Path.GetFullPath(path);
                if (string.IsNullOrEmpty(FullPath)) // Path.GetFullPath can return empty if input is effectively empty after processing
                {
                    // This case might be redundant if Path.GetFullPath throws for such inputs,
                    // but kept for robustness based on original logic.
                    Loggers.SystemLogger.Error("FileSystemBase.Constructor: Path.GetFullPath returned null or empty for input '{InitialPath}'. Using original processed path.", originalPath);
                    FullPath = path; // Fallback to the processed path.
                }
            }
            catch (ArgumentException ex)
            {
                Loggers.SystemLogger.Error(ex, "FileSystemBase.Constructor: Error obtaining full path for input '{InitialPath}'.", originalPath);
                throw new ArgumentException($"Invalid path specified: {originalPath}", ex);
            }
            catch (SecurityException ex)
            {
                Loggers.SystemLogger.Error(ex, "FileSystemBase.Constructor: Error obtaining full path for input '{InitialPath}'.", originalPath);
                throw new ArgumentException($"Invalid path specified: {originalPath}", ex);
            }
            catch (NotSupportedException ex)
            {
                Loggers.SystemLogger.Error(ex, "FileSystemBase.Constructor: Error obtaining full path for input '{InitialPath}'.", originalPath);
                throw new ArgumentException($"Invalid path specified: {originalPath}", ex);
            }
            catch (PathTooLongException ex)
            {
                Loggers.SystemLogger.Error(ex, "FileSystemBase.Constructor: Error obtaining full path for input '{InitialPath}'.", originalPath);
                throw new ArgumentException($"Invalid path specified: {originalPath}", ex);
            }
        }

        /// <summary>
        /// The full raw path to this file or directory.
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// Gets a value indicating whether the current user has write permissions to this file system entry.
        /// For directories, this is tested by attempting to create and delete a temporary file within the directory.
        /// For files, it attempts to open the file with write access.
        /// </summary>
        public virtual bool Writable
        {
            get
            {
                string? testFilePath = null;
                try
                {
                    var directoryInfo = new DirectoryInfo(FullPath);
                    var fileInfo = new FileInfo(FullPath);
                    if (fileInfo.Exists)
                    {
                        using (File.Open(FullPath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                        {
                            return true;
                        }
                    }
                    else // Assuming it's a directory or a file that doesn't exist yet (and we're checking dir writability)
                    {
                        // Ensure the directory exists if we are testing directory writability
                        // If FullPath points to a non-existent file in an existing directory, this will test the directory.
                        // If FullPath points to a non-existent file in a non-existent directory, DirectoryInfo will handle it.
                        string targetDirectory = directoryInfo.Exists ? FullPath : fileInfo.DirectoryName;
                        if (string.IsNullOrEmpty(targetDirectory) || !Directory.Exists(targetDirectory))
                        {
                             // If the directory itself doesn't exist, can't write a test file.
                             // This could be a new file in a new directory. The ability to create the directory
                             // would be a separate check. For now, if dir doesn't exist, assume not writable here.
                            Loggers.SystemLogger.Debug("FileSystemBase.Writable: Target directory {TargetDirectory} for path {FullPath} does not exist. Cannot perform write test.", targetDirectory, FullPath);
                            return false;
                        }


                        testFilePath = Path.Combine(targetDirectory, Guid.NewGuid().ToString() + ".tmp");
                        using (File.Create(testFilePath))
                        {
                            return true;
                        }
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    Loggers.SystemLogger.Warning(ex, "FileSystemBase.Writable: UnauthorizedAccessException checking write access for {FullPath}.", FullPath);
                    return false;
                }
                catch (IOException ex)
                {
                    Loggers.SystemLogger.Warning(ex, "FileSystemBase.Writable: IOException checking write access for {FullPath}.", FullPath);
                    return false;
                }
                finally
                {
                    if (testFilePath != null && File.Exists(testFilePath))
                    {
                        try
                        {
                            File.Delete(testFilePath);
                        }
                        catch (Exception ex) // Catch specific exceptions if needed, but Exception covers all.
                        {
                            Loggers.SystemLogger.Warning(ex, "FileSystemBase.Writable: Failed to delete temporary test file {TestFilePath} for path {FullPath}.", testFilePath, FullPath);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current FileSystemBase object based on FullPath.
        /// </summary>
        public override bool Equals(object? obj)
        {
            return Equals(obj as FileSystemBase);
        }

        /// <summary>
        /// Determines whether the specified FileSystemBase object is equal to the current FileSystemBase object based on FullPath.
        /// </summary>
        public bool Equals(FileSystemBase? other)
        {
            return other is not null &&
                   FullPath == other.FullPath;
        }

        /// <summary>
        /// Serves as the default hash function, using the hash code of the FullPath.
        /// </summary>
        public override int GetHashCode()
        {
            return FullPath.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current object, which is its FullPath.
        /// </summary>
        public override string? ToString()
        {
            return FullPath;
        }

        /// <summary>
        /// Compares two FileSystemBase objects for equality based on their FullPath.
        /// </summary>
        public static bool operator ==(FileSystemBase? left, FileSystemBase? right)
        {
            return EqualityComparer<FileSystemBase>.Default.Equals(left, right);
        }

        /// <summary>
        /// Compares two FileSystemBase objects for inequality based on their FullPath.
        /// </summary>
        public static bool operator !=(FileSystemBase? left, FileSystemBase? right)
        {
            return !(left == right);
        }
    }
}