using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace BLAZAM.FileSystem
{
    public class SystemFile : FileSystemBase
    {
        public SystemFile(string path) : base(path)
        {
        }
        public bool Exists => File.Exists(FullPath);


        public string Name => System.IO.Path.GetFileNameWithoutExtension(FullPath);
        public string Extension => System.IO.Path.GetExtension(FullPath);
        public SystemDirectory ParentDirectory => new SystemDirectory(System.IO.Path.GetDirectoryName(FullPath));

        public async Task<byte[]> ReadAllBytesAsync()
        {
            return await File.ReadAllBytesAsync(FullPath);
        }
        public byte[] ReadAllBytes()
        {
            return File.ReadAllBytes(FullPath);
        }
        public string ReadAllText()
        {
            return File.ReadAllText(FullPath);
        }
        public bool WriteAllText(string? text)
        {
            File.WriteAllText(FullPath, text);
            return true;
        }

        public DateTime LastModified { get => File.GetLastWriteTime(FullPath); }

        public TimeSpan SinceLastModified { get => DateTime.Now - LastModified; }
        /// <summary>
        /// Checks if the directory this file is in is writable
        /// </summary>
        /// <remarks>
        /// This does not check if the file itself is writable
        /// </remarks>
        public override bool Writable
        {
            get
            {
                if (this.Exists) return base.Writable;
                return ParentDirectory.Writable;
            }
        }

        public void Delete()
        {
            File.Delete(FullPath);
        }

        public FileStream OpenReadStream()
        {
            return new FileStream(FullPath, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize: 4096, useAsync: true);
        }
        /// <summary>
        /// Returns an opened stream reader to this file
        /// </summary>
        /// <returns></returns>
        public FileStream OpenWriteStream()
        {
            return new FileStream(FullPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
        }

        /// <summary>
        /// Creates the file if it does not already exist
        /// </summary>
        public void EnsureCreated()
        {
            if (!Exists)
                Create();
        }
        /// <summary>
        /// Creates this file with no bytes
        /// </summary>
        private void Create()
        {
            if (!ParentDirectory.Exists)
            {
                ParentDirectory.EnsureCreated();
            }
            var stream = new FileStream(FullPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None, bufferSize: 4096, useAsync: true);
            stream.Close();
        }
    }
}