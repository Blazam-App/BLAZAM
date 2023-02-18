using Microsoft.IdentityModel.Tokens;

namespace BLAZAM
{
    public class FileSystemBase
    {
        public FileSystemBase(string path)
        {
            Path = System.IO.Path.GetFullPath(path);
            if (Path.IsNullOrEmpty())
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