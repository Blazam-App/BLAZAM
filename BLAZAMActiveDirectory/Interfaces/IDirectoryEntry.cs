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
        string Path { get; }
        string? NativeGuid { get; }
        string Name { get; }
        IDirectoryEntry Parent { get; }
        IDirectoryEntries Children { get; }

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

        AuthType AuthenticationType { get; }
        CipherAlgorithmType EncryptionType { get; }
        bool SslEnabled { get; }
        bool UsePropertyCache { get; set; } 
        string? DN { get; set; }
       
    }
}
