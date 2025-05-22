using BLAZAM.ActiveDirectory.Interfaces;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class AdapterDirectoryEntry : IDirectoryEntry
    {
        public readonly DirectoryEntry UnderlyingEntry;
        private bool disposedValue;

        public AdapterDirectoryEntry(DirectoryEntry underlyingEntry)
        {
            UnderlyingEntry = underlyingEntry;
        }

        public string Path { get => UnderlyingEntry.Path; set => UnderlyingEntry.Path = value; }

        public string? NativeGuid => UnderlyingEntry.NativeGuid;

        public PropertyCollection Properties => UnderlyingEntry.Properties;

        public string SchemaClassName => UnderlyingEntry.SchemaClassName;

        public string Name => UnderlyingEntry.Name;

        public IDirectoryEntry Parent
        {
            get
            {
                return new AdapterDirectoryEntry(UnderlyingEntry.Parent);
            }
        }

        public DirectoryEntries Children => UnderlyingEntry.Children;

        public AuthenticationTypes AuthenticationType { get => UnderlyingEntry.AuthenticationType; set => UnderlyingEntry.AuthenticationType = value; }
        public bool UsePropertyCache { get => UnderlyingEntry.UsePropertyCache; set => UnderlyingEntry.UsePropertyCache = value; }

        public void Close()
        {
            UnderlyingEntry.Close();
        }

        public void CommitChanges()
        {
            UnderlyingEntry.CommitChanges();
        }

        public object Invoke(string methodName, params object[]? args)
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
    }
}
