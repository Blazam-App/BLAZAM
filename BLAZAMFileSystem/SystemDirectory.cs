using BLAZAM.Logger;
using System.Security; // Added for SecurityException


namespace BLAZAM.FileSystem
{
    /// <summary>
    /// Represents a directory in the file system, providing properties and methods for directory manipulation.
    /// </summary>
    public class SystemDirectory : FileSystemBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemDirectory"/> class.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <exception cref="ArgumentException">Thrown if the path is invalid.</exception>
        public SystemDirectory(string path) : base(path) // base constructor handles initial Path.GetFullPath and %temp%
        {
            // The base constructor already called Path.GetFullPath.
            // We need to ensure this path ends with a directory separator.
            // If base.FullPath is already correct and ends with a separator, this might be redundant,
            // but it ensures consistency for SystemDirectory instances.
            string originalPathForLogging = path; // Use the original path for logging if GetFullPath fails here
            try
            {
                // Ensure the path from base is treated as a directory path.
                // Path.GetFullPath might not add a trailing slash if the directory doesn't exist.
                // We append one to signify it's a directory for consistency in FullPath.
                string basePath = base.FullPath; // Get FullPath from base constructor
                if (!string.IsNullOrEmpty(basePath) &&
                    !basePath.EndsWith(Path.DirectorySeparatorChar.ToString()) &&
                    !basePath.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                {
                    FullPath = basePath + Path.DirectorySeparatorChar;
                }
                else
                {
                    FullPath = basePath;
                }
                // Re-evaluate FullPath to ensure it's absolute and clean after potential modification
                if (!string.IsNullOrEmpty(FullPath))
                {
                    FullPath = Path.GetFullPath(FullPath);
                }
            }
            catch (ArgumentException ex)
            {
                Loggers.SystemLogger.Error(ex, "SystemDirectory.Constructor: Error obtaining full path for input '{InitialPath}'.", originalPathForLogging);
                throw new ArgumentException($"Invalid directory path specified: {originalPathForLogging}", ex);
            }
            catch (SecurityException ex)
            {
                Loggers.SystemLogger.Error(ex, "SystemDirectory.Constructor: Error obtaining full path for input '{InitialPath}'.", originalPathForLogging);
                throw new ArgumentException($"Invalid directory path specified: {originalPathForLogging}", ex);
            }
            catch (NotSupportedException ex)
            {
                Loggers.SystemLogger.Error(ex, "SystemDirectory.Constructor: Error obtaining full path for input '{InitialPath}'.", originalPathForLogging);
                throw new ArgumentException($"Invalid directory path specified: {originalPathForLogging}", ex);
            }
            catch (PathTooLongException ex)
            {
                Loggers.SystemLogger.Error(ex, "SystemDirectory.Constructor: Error obtaining full path for input '{InitialPath}'.", originalPathForLogging);
                throw new ArgumentException($"Invalid directory path specified: {originalPathForLogging}", ex);
            }
        }

        /// <summary>
        /// Gets a list of all direct sub-directories within this directory. Returns an empty list if the directory does not exist or an error occurs.
        /// </summary>
        public List<SystemDirectory> SubDirectories
        {
            get
            {
                List<SystemDirectory> dirs = [];
                try
                {
                    if (Exists)
                    {
                        foreach (var directory in Directory.GetDirectories(FullPath))
                        {
                            dirs.Add(new SystemDirectory(directory));
                        }
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    //Ignore directories not found as they are the . and .. directories
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Error(ex, "SystemDirectory.SubDirectories: Error getting subdirectories for {Path}.", FullPath);
                }
                return dirs;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the directory currently exists.
        /// </summary>
        public bool Exists => Directory.Exists(FullPath);

        /// <summary>
        /// Gets a list of all direct files within this directory. Returns an empty list if the directory does not exist or an error occurs.
        /// </summary>
        public List<SystemFile> Files
        {
            get
            {
                List<SystemFile> files = [];
                try
                {
                    if (Exists)
                    {
                        foreach (var file in Directory.GetFiles(FullPath))
                        {
                            files.Add(new SystemFile(file));
                        }
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    // Ignore if directory not found during file listing
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Error(ex, "SystemDirectory.Files: Error getting files for {Path}.", FullPath);
                }
                return files;
            }
        }
        public List<SystemFile> GetFilesAndSubFiles(string? filter = "*.*")
        {


            List<SystemFile> files = [];
            try
            {
                if (Exists)
                {
                    // Get all files in the directory and subdirectories
                    foreach (var file in Directory.GetFiles(FullPath, filter, SearchOption.AllDirectories))
                    {
                        files.Add(new SystemFile(file));
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                // Ignore if directory not found during file listing
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "SystemDirectory.FilesAndSubFiles: Error getting files for {Path}.", FullPath);
            }
            return files;
        }
        /// <summary>
        /// Gets the name of the directory (the last part of the path). Returns null or empty if FullPath is invalid.
        /// </summary>
        public string? Name
        {
            get
            {
                if (string.IsNullOrEmpty(FullPath))
                {
                    Loggers.SystemLogger.Warning("SystemDirectory.Name: FullPath is null or empty. Cannot determine directory name.");
                    return null;
                }
                // More robust way to get directory name
                return Path.GetFileName(FullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            }
        }

        /// <summary>
        /// Deletes all files directly within this directory. Logs warnings for files that cannot be deleted but continues operation.
        /// </summary>
        public void ClearDirectory()
        {
            var fileList = new List<SystemFile>(Files); // Create a copy to iterate over, as Files property might change
            foreach (var file in fileList)
            {
                try
                {
                    file.Delete();
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Warning(ex, "SystemDirectory.ClearDirectory: Failed to delete file {FilePath} in directory {DirectoryPath}.", file.FullPath, FullPath);
                    // Continue to next file
                }
            }
        }

        /// <summary>
        /// Copies the entire directory tree (all files and subdirectories) to the specified target directory. Overwrites files if they exist.
        /// </summary>
        /// <param name="parentDirectory">The target directory to copy into.</param>
        /// <param name="onProgress">Optional progress tracker callback.</param>
        /// <returns>True if the copy operation completes without critical errors; false otherwise.</returns>
        public bool CopyTo(SystemDirectory parentDirectory, IProgress<FileProgress>? onProgress = null)
        {
            if (parentDirectory == null)
            {
                Loggers.SystemLogger.Error("SystemDirectory.CopyTo: parentDirectory parameter is null for source {SourcePath}.", FullPath);
                return false;
            }

            bool copyingDownTree = parentDirectory.FullPath.Contains(FullPath);
            if (!Exists)
            {
                Loggers.SystemLogger.Warning("SystemDirectory.CopyTo: Source directory {SourcePath} does not exist. Cannot copy.", FullPath);
                return false;
            }

            bool anyError = false;

            // Handle Directories
            var directories = Directory.GetDirectories(FullPath, "*", SearchOption.AllDirectories).AsEnumerable();
            if (copyingDownTree)
                directories = directories.Where(d => !d.Contains(parentDirectory.FullPath));

            foreach (string dirPath in directories)
            {
                try
                {
                    Directory.CreateDirectory(dirPath.Replace(FullPath, parentDirectory.FullPath));
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Error(ex, "SystemDirectory.CopyTo: Failed to create destination subdirectory {DestDirPath} when copying from {SourcePath}.", dirPath.Replace(FullPath, parentDirectory.FullPath), FullPath);
                    anyError = true;
                }
            }

            // Handle Files
            var files = Directory.GetFiles(FullPath, "*.*", SearchOption.AllDirectories).AsEnumerable();
            if (copyingDownTree)
                files = files.Where(f => !f.Contains(parentDirectory.FullPath));

            var filesList = files.ToList();
            
            // 1. Calculate the total size of all files going to be copied
            long totalSize = 0;
            foreach (var f in filesList)
            {
                totalSize += new FileInfo(f).Length;
            }

            FileProgress progress = new FileProgress
            {
                ExpectedSize = totalSize,
                CompletedBytes = 0
            };

            foreach (string newPath in filesList)
            {
                try
                {
                    FileInfo fInfo = new FileInfo(newPath);
                    string destPath = newPath.Replace(FullPath, parentDirectory.FullPath);

                    File.Copy(newPath, destPath, true);

                    // 2. Report progress after the file copies successfully
                    progress.CompletedBytes += fInfo.Length;
                    onProgress?.Report(progress);
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Error(ex, "SystemDirectory.CopyTo: Failed to copy file {SourceFilePath} to {DestFilePath}.", newPath, newPath.Replace(FullPath, parentDirectory.FullPath));
                    anyError = true;
                }
            }
            return !anyError;
        }

        /// <summary>
        /// Deletes this directory. Logs an error if the operation fails.
        /// </summary>
        /// <param name="recursive">If true, deletes all subdirectories and files; otherwise, the directory must be empty.</param>
        public void Delete(bool recursive = false)
        {
            if (Exists)
            {
                try
                {
                    Directory.Delete(FullPath, recursive);
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Error(ex, "SystemDirectory.Delete: Failed to delete directory {DirectoryPath}. Message: {ErrorMessage}", FullPath, ex.Message);
                }
            }
        }

        /// <summary>
        /// Ensures that the directory exists. If it does not, it attempts to create it. Logs an error if creation fails.
        /// </summary>
        public void EnsureCreated()
        {
            try
            {
                Directory.CreateDirectory(FullPath);
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "SystemDirectory.EnsureCreated: Failed to create directory {DirectoryPath}. Message: {ErrorMessage}", FullPath, ex.Message);
            }
        }
    }
}
