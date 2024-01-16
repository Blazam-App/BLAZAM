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
   
            path = path.Replace("%temp%", System.IO.Path.GetTempPath());
            Path = System.IO.Path.GetFullPath(path);
            if (Path==null || Path=="")
                Path = path;
        }

        public string Path { get; set; }

        public virtual bool Writable
        {
            get
            {
                string? testFilePath = null;
                try
                {
                    var directoryInfo = new DirectoryInfo(Path);
                    var fileInfo = new FileInfo(Path);
                    if (fileInfo.Exists)
                    {
                        using (File.Open(Path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                        {
                            return true;
                        }

                    }
                    else
                    {
                        //if (!directoryInfo.Exists) throw new DirectoryNotFoundException("Directory " + Path + " does not exist!");

                        testFilePath = Path + "test.txt";
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
                        File.Delete(testFilePath);
                    }
                }
            }
        }


        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public override string? ToString()
        {
            return Path;
        }
    }
}