using BLAZAM.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM
{
    public class SystemDirectory : FileSystemBase
    {
        public SystemDirectory(string path) : base(path)
        {

        }

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

                }
                catch (Exception ex)
                {
                    Log.Error("Error getting directory files: " + Path, ex);
                }
                return dirs;
            }
        }
        public bool Exists => Directory.Exists(Path);

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

        public string Name => System.IO.Path.GetDirectoryName(Path);


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
        public bool Writable
        {
            get
            {
                string randomFileName = Path + "\\" + Guid.NewGuid().ToString()+".txt";
                try
                {
                    File.WriteAllText(randomFileName, "test");
                }catch (Exception e)
                {
                    return false;
                }
                File.Delete(randomFileName);
                return true;
            }
        }
        public void Delete(bool recursive = false)
        {
            Directory.Delete(Path, recursive);
        }

        public void EnsureCreated()
        {
            Directory.CreateDirectory(Path);
        }
    }
}
