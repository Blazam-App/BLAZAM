using BLAZAM.Logger;


namespace BLAZAM.FileSystem
{
    /// <summary>
    /// Represents a directory in the file system, providing properties and methods for directory manipulation.
    /// </summary>
    public class SystemDirectory : FileSystemBase, IFileSystemObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemDirectory"/> class.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <exception cref="ArgumentException">Thrown if the path is invalid.</exception>
        public SystemDirectory(string path) : base(path) // base constructor handles initial Path.GetFullPath and %temp%
        {

        }

        /// <summary>
        /// Gets a list of all direct sub-directories within this directory. Returns an empty list if the directory does not exist or an error occurs.
        /// </summary>
        public List<SystemDirectory> SubDirectories
        {
            get
            {
                List<SystemDirectory> dirs = new();
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
                List<SystemFile> files = new();
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

        public SystemDirectory ParentDirectory
        {
            get
            {
                try
                {
                    // Normalize path separators for cross-platform compatibility
                    var trimmedPath = FullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                    // Get parent directory path
                    var parentPath = Path.GetDirectoryName(trimmedPath);

                    // If parentPath is null, this is a root directory (e.g., "/" or "C:\")
                    if (string.IsNullOrEmpty(parentPath))
                    {
                        // For UNC paths, return the share root if possible
                        if (Path.IsPathRooted(FullPath) && FullPath.StartsWith(@"\\"))
                        {
                            // UNC root: \\server\share
                            var uncParts = FullPath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                            if (uncParts.Length >= 2)
                            {
                                var uncRoot = $@"\\{uncParts[0]}\{uncParts[1]}";
                                return new SystemDirectory(uncRoot);
                            }
                        }
                        // For local root, return itself
                        return new SystemDirectory(FullPath);
                    }

                    return new SystemDirectory(parentPath);
                }
                catch
                {
                    // On error, return itself as a fallback
                    return new SystemDirectory(FullPath);
                }
            }
        }

        public List<SystemFile> GetFilesAndSubFiles(string? filter = "*.*")
        {


            List<SystemFile> files = new();
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
        /// <returns>True if the copy operation completes without critical errors (some individual file copy errors might be logged but allow continuation); false otherwise.</returns>
        public bool CopyTo(SystemDirectory parentDirectory)
        {
            if (parentDirectory == null)
            {
                Loggers.SystemLogger.Error("SystemDirectory.CopyTo: parentDirectory parameter is null for source {SourcePath}.", FullPath);
                return false;
            }

            bool copyingDownTree = false;
            if (parentDirectory.FullPath.Contains(FullPath))
            {
                copyingDownTree = true;
            }

            if (!Exists)
            {
                Loggers.SystemLogger.Warning("SystemDirectory.CopyTo: Source directory {SourcePath} does not exist. Cannot copy.", FullPath);
                return false;
            }

            bool anyError = false;

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
                    anyError = true; // Mark error, but continue if possible
                }
            }

            var files = Directory.GetFiles(FullPath, "*.*", SearchOption.AllDirectories).AsEnumerable();
            if (copyingDownTree)
                files = files.Where(f => !f.Contains(parentDirectory.FullPath));

            foreach (string newPath in files)
            {
                try
                {
                    File.Copy(newPath, newPath.Replace(FullPath, parentDirectory.FullPath), true);
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Error(ex, "SystemDirectory.CopyTo: Failed to copy file {SourceFilePath} to {DestFilePath}.", newPath, newPath.Replace(FullPath, parentDirectory.FullPath));
                    anyError = true; // Mark error, but continue
                }
            }
            return !anyError;
        }
        public void Delete()
        {
            Delete(false);
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
        public bool Create()
        {
            try
            {
                Directory.CreateDirectory(FullPath);
                return Exists;
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "SystemDirectory.EnsureCreated: Failed to create directory {DirectoryPath}. Message: {ErrorMessage}", FullPath, ex.Message);
            }
            return false;
        }

        public override bool Rename(string newName)
        {
            try
            {
                var parent = Path.GetDirectoryName(FullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                if (parent == null) return false;
                var newPath = Path.Combine(parent, newName);
                Directory.Move(FullPath, newPath);
                FullPath = newPath;
                return true;
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Error renaming directory {Directory} to {NewName}", FullPath, newName);
                return false;
            }
        }
    }
}
