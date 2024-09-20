

using Serilog;

namespace BLAZAM.FileSystem
{
    /// <summary>
    /// Represents a directory in the filesystem
    /// </summary>
    public class SystemDirectory : FileSystemBase
    {
        public SystemDirectory(string path) : base(path)
        {
            FullPath = Path.GetFullPath(path + Path.DirectorySeparatorChar);
        }
        /// <summary>
        /// All direct sub-directories of this directory
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
                    Log.Error("Error getting directory files: " + FullPath, ex);
                }
                return dirs;
            }
        }
        /// <summary>
        /// Indicates whether this directory currently exists
        /// </summary>
        public bool Exists => Directory.Exists(FullPath);

        /// <summary>
        /// All direct sub-files of this directory
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

                }
                catch (Exception ex)
                {
                    Log.Error("Error getting directory files: " + FullPath, ex);
                }
                return files;
            }
        }

        /// <summary>
        /// The full directory name
        /// </summary>
        public string? Name => FullPath.Split("\\").Last();

        public void ClearDirectory()
        {
            var fileList = new List<SystemFile>(Files);
            foreach (var file in fileList)
            {
                file.Delete();
            }
        }


        /// <summary>
        /// Copies the entire directory tree to another directory
        /// </summary>
        /// <param name="parentDirectory"></param>
        /// <returns></returns>
        public bool CopyTo(SystemDirectory parentDirectory)
        {
            bool copyingDownTree = false;
            if (parentDirectory.FullPath.Contains(FullPath))
            {
                copyingDownTree = true;
            }

            if (Exists)
            {

                var directories = Directory.GetDirectories(FullPath, "*", SearchOption.AllDirectories).AsEnumerable();

                if (copyingDownTree)
                    directories = directories.Where(d => !d.Contains(parentDirectory.FullPath));

                //Now Create all of the directories
                foreach (string dirPath in directories)
                {
                    Directory.CreateDirectory(dirPath.Replace(FullPath, parentDirectory.FullPath));
                }
                var files = Directory.GetFiles(FullPath, "*.*", SearchOption.AllDirectories).AsEnumerable();

                if (copyingDownTree)
                    files = files.Where(f => !f.Contains(parentDirectory.FullPath));
                //Copy all the files & Replaces any files with the same name
                foreach (string newPath in files)
                {
                    File.Copy(newPath, newPath.Replace(FullPath, parentDirectory.FullPath), true);
                }
                return true;

            }
            return false;
        }
        /// <summary>
        /// Deletes this directory
        /// </summary>
        /// <param name="recursive"></param>
        public void Delete(bool recursive = false)
        {
            if (Exists)
            {
                Directory.Delete(FullPath, recursive);
            }
        }
        /// <summary>
        /// Creates the directory if it does not already exist
        /// </summary>
        public void EnsureCreated()
        {
            Directory.CreateDirectory(FullPath);
        }
    }
}
