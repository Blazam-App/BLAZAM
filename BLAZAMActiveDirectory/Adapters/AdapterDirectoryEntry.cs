
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Helpers;
using System.Collections;
using System.DirectoryServices;

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
        public bool PropertyContains(string propertyName,object value)
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
                return new AdapterDirectoryEntry(UnderlyingEntry.Parent);
            }
        }

        public IDirectoryEntries Children => new AdapterDirectoryEntries(UnderlyingEntry.Children);

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

    /// <summary>
    /// Defines a contract for a collection of directory entries,
    /// compatible with System.DirectoryServices.DirectoryEntries.
    /// </summary>
    public interface IDirectoryEntries : IEnumerable
    {


        /// <summary>
        /// Creates a new entry in this collection.
        /// </summary>
        /// <param name="name">The name of the new entry.</param>
        /// <param name="schemaClassName">The schema class of the new entry.</param>
        /// <returns>The newly created <see cref="IDirectoryEntry"/>.</returns>
        IDirectoryEntry Add(string name, string schemaClassName);

        /// <summary>
        /// Returns the entry at the specified path in this collection.
        /// </summary>
        /// <param name="name">The name of the entry to find.</param>
        /// <param name="schemaClassName">The schema class of the entry to find. If null, any schema class is searched.</param>
        /// <returns>The <see cref="IDirectoryEntry"/> found.</returns>
        IDirectoryEntry Find(string name, string schemaClassName);

        /// <summary>
        /// Returns the entry at the specified path in this collection.
        /// </summary>
        /// <param name="name">The name of the entry to find.</param>
        /// <returns>The <see cref="IDirectoryEntry"/> found.</returns>
        IDirectoryEntry Find(string name);

        /// <summary>
        /// Removes the specified entry from this collection.
        /// </summary>
        /// <param name="entry">The <see cref="IDirectoryEntry"/> to remove.</param>
        void Remove(IDirectoryEntry entry);
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
}
