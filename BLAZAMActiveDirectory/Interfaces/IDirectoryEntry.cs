using BLAZAM.ActiveDirectory.Adapters;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    public interface IDirectoryEntry : IDisposable
    {
        /// <summary>
        /// The full ADS path to this object
        /// </summary>
        string Path { get; }
        /// <summary>
        /// The native GUID of the object, if available. This is not guaranteed to be present for all directory types, and may be null if the underlying directory does not support it or if it cannot be retrieved. It is recommended to use this property for uniquely identifying objects when available, but fallback to other properties like Path or Name when necessary.
        /// </summary>
        string? NativeGuid { get; }
        /// <summary>
        /// The name of the directory entry, which is typically the last component of the Path. This is not guaranteed to be unique across the directory, and may not be present for all directory types. It is recommended to use this property for display purposes, but not as a unique identifier.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The direct parent Active Directory object for this object.
        /// </summary>
        IDirectoryEntry Parent { get; }
        IDirectoryEntries Children { get; }
        /// <summary>
        /// Commits any pending changes to the directory.
        /// </summary>
        void CommitChanges();
        void RefreshCache();
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
        List<object?> GetNonReplicatedPropertyValue(string propertyName);

        AuthType AuthenticationType { get; }
        CipherAlgorithmType EncryptionType { get; }
        bool SslEnabled { get; }
        bool UsePropertyCache { get; set; } 
        string? DN { get; set; }
       
    }
}
