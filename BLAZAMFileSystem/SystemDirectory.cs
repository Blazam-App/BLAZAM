

using Serilog;
using System.IO;
using System.Security.Permissions;
using System.Security;

namespace BLAZAM.FileSystem
{
    /// <summary>
    /// Represents a diretory in the filesystem
    /// </summary>
    public class SystemDirectory : FileSystemBase
    {
        public SystemDirectory(string path) : base(path)
        {

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
                    if (Directory.Exists(Path))
                    {
                        foreach (var directory in Directory.GetDirectories(Path))
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
                    Log.Error("Error getting directory files: " + Path, ex);
                }
                return dirs;
            }
        }
        /// <summary>
        /// Indicates whether this directory currently exists
        /// </summary>
        public bool Exists => Directory.Exists(Path);

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
                    if (Directory.Exists(Path))
                    {
                        foreach (var file in Directory.GetFiles(Path))
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
                    Log.Error("Error getting directory files: " + Path, ex);
                }
                return files;
            }
        }

        /// <summary>
        /// The full directory name
        /// </summary>
        public string? Name => System.IO.Path.GetDirectoryName(Path);

        public void ClearDirectory()
        {
            var fileList = new List<SystemFile>(Files);
            foreach(var file in fileList)
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
            if (parentDirectory.Path.Contains(Path))
            {
                copyingDownTree = true;
            }

            if (Directory.Exists(Path))
            {

                var directories = Directory.GetDirectories(Path, "*", SearchOption.AllDirectories).AsEnumerable();

                if (copyingDownTree)
                    directories = directories.Where(d => !d.Contains(parentDirectory.Path));

                //Now Create all of the directories
                foreach (string dirPath in directories)
                {
                    Directory.CreateDirectory(dirPath.Replace(Path, parentDirectory.Path));
                }
                var files = Directory.GetFiles(Path, "*.*", SearchOption.AllDirectories).AsEnumerable();

                if (copyingDownTree)
                    files = files.Where(f => !f.Contains(parentDirectory.Path));
                //Copy all the files & Replaces any files with the same name
                foreach (string newPath in files)
                {
                    File.Copy(newPath, newPath.Replace(Path, parentDirectory.Path), true);
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
            Directory.Delete(Path, recursive);
        }
        /// <summary>
        /// Creates the directory if it does not already exist
        /// </summary>
        public void EnsureCreated()
        {
            Directory.CreateDirectory(Path);
        }
    }
}
