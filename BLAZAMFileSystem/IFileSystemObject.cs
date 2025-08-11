
namespace BLAZAM.FileSystem
{
    public interface IFileSystemObject
    {
        bool Exists { get; }
        string? Name { get; }
        string FullPath { get; set; }
        SystemDirectory ParentDirectory { get; }

        bool CopyTo(SystemDirectory parentDirectory);
        void Delete();
        bool Rename(string newName);
        bool Create();
    }
}