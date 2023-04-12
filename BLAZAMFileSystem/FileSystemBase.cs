﻿
namespace BLAZAM.FileSystem
{
    public class FileSystemBase
    {
        public FileSystemBase(string path)
        {
            path = path.Replace("%temp%", System.IO.Path.GetTempPath());
            Path = System.IO.Path.GetFullPath(path);
            if (Path==null || Path=="")
                Path = path;
        }

        public string Path { get; set; }


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