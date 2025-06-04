using BLAZAM.ActiveDirectory.Adapters;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    public interface IDirectoryEntry : IDisposable
    {
        string Path { get; set; }
        string? NativeGuid { get; }
        string SchemaClassName { get; }
        string Name { get; }
        IDirectoryEntry Parent { get; }
        IDirectoryEntries Children { get; }

        void CommitChanges();
        void RefreshCache();
        void Close();
        object? Invoke(string methodName, params object[]? args);
        void Rename(string newName);
        void MoveTo(IDirectoryEntry newParent);
        void SetPropertyValue(string propertyName, object? value);
        object? GetPropertyValue(string propertyName);
        bool ContainsProperty(string propertyName);
        void ClearPropertyValue(string propertyName);
        void RemovePropertyValue(string propertyName, object? value);
        void AddPropertyValue(string propertyName, object? value);
        bool PropertyContains(string propertyName, object value);

        //void MoveTo(IDirectoryEntry newParent, string? newName); // System.DirectoryServices.DirectoryEntry also has this overload

        // Additional properties/methods based on potential usage in DirectoryEntryAdapter
        // bool Exists(string path); // Static method on DirectoryEntry, not suitable for interface instance method
        AuthType AuthenticationType { get; } // Property on DirectoryEntry
        bool UsePropertyCache { get; set; } // Property on DirectoryEntry
        string? DN { get; set; }
        // Options property (DirectoryEntryConfiguration Options) might be too complex to add unless needed
    }
}
