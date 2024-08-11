using Serilog.Configuration;
using System.Security;
using System.Security.AccessControl;
using System.Security.Permissions;

namespace BLAZAM.FileSystem
{
    public class FileSystemBase
    {
        public FileSystemBase(string path)
        {
            if (path is null) 
                throw new ArgumentException("path parameter should not be null");
   
            path = path.Replace("%temp%", Path.GetTempPath());
            FullPath = Path.GetFullPath(path);
            if (FullPath==null || FullPath=="")
                FullPath = path;
        }
        /// <summary>
        /// The full raw path to this file or directory
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// Indicates whether the executing identity has write permission to this directory or file
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
                    else
                    {
                        //if (!directoryInfo.Exists) throw new DirectoryNotFoundException("Directory " + Path + " does not exist!");

                        testFilePath = System.IO.Path.GetFullPath(FullPath + "\\test.txt");
                        // Attempt to create a test file within the directory.
                        
                        using (File.Create(testFilePath))
                        {
                            // If the file can be created, it indicates write permissions on the directory.
                            return true;
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Handle unauthorized access or log an error as needed
                    return false;
                }
                catch (IOException)
                {
                    // Handle other IO exceptions or log an error as needed

                    return false;
                }
                finally
                {
                    // Clean up the test file if it was created
                    if (testFilePath != null && File.Exists(testFilePath))
                    {
                        try
                        {
                            File.Delete(testFilePath);
                        }
                        catch
                        {
                            //Do nothing if we can't delete the test file
                        }
                    }
                }
            }
        }


        public override int GetHashCode()
        {
            return FullPath.GetHashCode();
        }

        public override string? ToString()
        {
            return FullPath;
        }
    }
}