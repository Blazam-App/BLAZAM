using System.Security;
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
                try
                {
                    var permissionSet = new PermissionSet(PermissionState.None);
                    var writePermission = new FileIOPermission(FileIOPermissionAccess.Write, Path);
                    permissionSet.AddPermission(writePermission);
                    permissionSet.Demand();
                    return true;
                }
                catch (SecurityException ex)
                {
                   //Loggers.SystemLogger.Warning(e.Message);

                    return false;
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