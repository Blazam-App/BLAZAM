
using System.Collections;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.Security.Authentication;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Helpers;
using BLAZAM.Logger;

namespace BLAZAM.ActiveDirectory.Adapters
{
#pragma warning disable CA1416 // Validate platform compatibility

    public class AdapterDirectoryEntry : IDirectoryEntry
    {
        public readonly DirectoryEntry UnderlyingEntry;
        protected IActiveDirectoryContext Directory { get; set; }
        private bool disposedValue;
        public AdapterDirectoryEntry(DirectoryEntry underlyingEntry)
        {
            UnderlyingEntry = underlyingEntry;
        }
        public AdapterDirectoryEntry(DirectoryEntry underlyingEntry, IActiveDirectoryContext directory)
        {
            UnderlyingEntry = underlyingEntry;
            Directory = directory;
        }
        public string? DN { get => GetPropertyValue("disinguishedName").ToString(); set { /*donothing*/ } }
        public string Path { get => UnderlyingEntry.Path; set => UnderlyingEntry.Path = value; }

        public string? NativeGuid => UnderlyingEntry.NativeGuid;

        public void SetPropertyValue(string propertyName, object? value)
        {
            UnderlyingEntry.Properties[propertyName].Value = value;
        }
        public void RemovePropertyValue(string propertyName, object? value)
        {
            UnderlyingEntry.Properties[propertyName].Remove(value);
        }
        public void AddPropertyValue(string propertyName, object? value)
        {
            UnderlyingEntry.Properties[propertyName].Add(value);
        }
        public void ClearPropertyValue(string propertyName)
        {
            UnderlyingEntry.Properties[propertyName].Clear();
        }
        public bool ContainsProperty(string propertyName)
        {
            return UnderlyingEntry.Properties.Contains(propertyName);
        }
        public bool PropertyContains(string propertyName, object value)
        {
            return UnderlyingEntry.Properties[propertyName].Contains(value);
        }
        public object? GetPropertyValue(string propertyName)
        {
            return UnderlyingEntry.Properties[propertyName].Value;
        }
        public string SchemaClassName => UnderlyingEntry.SchemaClassName;

        public string Name => UnderlyingEntry.Name;

        public IDirectoryEntry Parent
        {
            get
            {
                return new AdapterDirectoryEntry(UnderlyingEntry.Parent, Directory);
            }
        }

        public IDirectoryEntries Children => new AdapterDirectoryEntries(UnderlyingEntry.Children);

        public AuthType AuthenticationType { get => (AuthType)UnderlyingEntry.AuthenticationType; }
        public CipherAlgorithmType EncryptionType { get => CipherAlgorithmType.None; }
        public bool SslEnabled { get => false; }
        public bool UsePropertyCache { get => UnderlyingEntry.UsePropertyCache; set => UnderlyingEntry.UsePropertyCache = value; }

        public void Close()
        {
            UnderlyingEntry.Close();
        }

        public void CommitChanges()
        {
            UnderlyingEntry.CommitChanges();
        }

        public object? Invoke(string methodName, params object[]? args)
        {
            return UnderlyingEntry.Invoke(methodName, args);
        }





        public void MoveTo(IDirectoryEntry newParent)
        {
            if (newParent is AdapterDirectoryEntry adapterDirectoryEntry)
            {
                UnderlyingEntry.MoveTo(adapterDirectoryEntry.UnderlyingEntry);

            }
        }

        public void RefreshCache()
        {
            UnderlyingEntry.RefreshCache();

        }

        public void Rename(string newName)
        {
            UnderlyingEntry.Rename(newName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    UnderlyingEntry.Dispose();
                }

                disposedValue = true;
            }
        }



        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }



        public class AdapterDirectoryEntries : IDirectoryEntries
        {
            public DirectoryEntries UnderlyingDirectoryEntries { get; private set; }

            public AdapterDirectoryEntries(DirectoryEntries underlyingDirectoryEntries)
            {
                UnderlyingDirectoryEntries = underlyingDirectoryEntries;
            }

            public IDirectoryEntry Add(string name, string schemaClassName)
            {
                return UnderlyingDirectoryEntries.Add(name, schemaClassName).ToIDirectoryEntry();
            }

            public IDirectoryEntry Find(string name, string schemaClassName)
            {
                return UnderlyingDirectoryEntries.Find(name, schemaClassName).ToIDirectoryEntry();
            }

            public IDirectoryEntry Find(string name)
            {
                return UnderlyingDirectoryEntries.Find(name).ToIDirectoryEntry();
            }

            public IEnumerator GetEnumerator()
            {
                return UnderlyingDirectoryEntries.GetEnumerator();
            }


            public void Remove(IDirectoryEntry entry)
            {
                if (entry is DirectoryEntry dEntry)
                    UnderlyingDirectoryEntries.Remove(dEntry);
            }
        }
#pragma warning restore CA1416 // Validate platform compatibility

    }
}
