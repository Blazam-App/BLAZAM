using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace BLAZAM
{
    public class SystemFile : FileSystemBase
    {
        public SystemFile(string path) : base(path)
        {
        }
        public bool Exists => File.Exists(Path);


        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);
        public string Extension => System.IO.Path.GetExtension(Path);
        public SystemDirectory ParentDirectory =>new SystemDirectory(System.IO.Path.GetDirectoryName(Path));

        public async Task<byte[]> ReadAllBytesAsync()
        {
            return await File.ReadAllBytesAsync(Path);
        }
        public byte[] ReadAllBytes()
        {
            return File.ReadAllBytes(Path);
        }
        public string ReadAllText()
        {
            return File.ReadAllText(Path);
        }
        public bool WriteAllText(string? text)
        {
            File.WriteAllText(Path, text);
            return true;
        }

        public DateTime LastModified { get => File.GetLastWriteTime(Path); }

        public TimeSpan SinceLastModified { get => DateTime.Now - LastModified; }
        public bool Writable
        {
            get
            {
                return ParentDirectory.Writable;
            }
        }

        public void Delete()
        {
            File.Delete(Path);
        }

        public FileStream OpenReadStream()
        {
            return new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize: 4096, useAsync: true);
        }

        public FileStream OpenWriteStream()
        {
            return new FileStream(Path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
        }

        public void EnsureCreated()
        {
            if (!Exists)
                Create();
        }

        private void Create()
        {
            var stream = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None, bufferSize: 4096, useAsync: true);
            stream.Close();
        }
    }
}