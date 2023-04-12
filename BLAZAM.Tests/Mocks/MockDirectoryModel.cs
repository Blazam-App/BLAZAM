
using BLAZAM.ActiveDirectory;
using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Database.Models;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Tests.Mocks
{
    internal class MockDirectoryModel : IDirectoryEntryAdapter
    {
        public string? SamAccountName
        {
            get => throw new NotImplementedException(); set => throw new NotImplementedException();
        }
        public string? CanonicalName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string? DN { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime? Created { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime? LastChanged { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public byte[]? SID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<string>? Classes { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool CanRead => throw new NotImplementedException();

        public ActiveDirectoryObjectType ObjectType => throw new NotImplementedException();

        public bool CanMove => throw new NotImplementedException();

        public bool CanRename => throw new NotImplementedException();

        public bool CanCreate => throw new NotImplementedException();

        public bool CanDelete => throw new NotImplementedException();

        public DirectoryEntry DirectoryEntry { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string? ADSPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool CanEdit => throw new NotImplementedException();

        public string? OU => throw new NotImplementedException();

        public bool HasUnsavedChanges => throw new NotImplementedException();

        public bool NewEntry { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string SearchUri => throw new NotImplementedException();

        public AppEvent? OnModelCommited { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public AppEvent? OnModelChanged { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Dictionary<string, object> NewEntryProperties { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IADOrganizationalUnit? LastKnownParent => throw new NotImplementedException();

        public bool IsDeleted => throw new NotImplementedException();

        public List<AuditChangeLog> Changes => throw new NotImplementedException();

        public AppEvent<IDirectoryEntryAdapter> OnDirectoryModelRenamed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool CanReadAnyCustomFields => throw new NotImplementedException();

        public AppEvent? OnModelDeleted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool CanEditField(IActiveDirectoryField field)
        {
            throw new NotImplementedException();
        }

        public bool CanReadField(IActiveDirectoryField field)
        {
            throw new NotImplementedException();
        }

        public DirectoryChangeResult CommitChanges()
        {
            throw new NotImplementedException();
        }

        public Task<DirectoryChangeResult> CommitChangesAsync()
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public void DiscardChanges()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void EnsureDirectoryEntry()
        {
            throw new NotImplementedException();
        }

        public T? GetCustomProperty<T>(string propertyName)
        {
            throw new NotImplementedException();
        }

        public DateTime? GetDateTimeProperty(string propertyName)
        {
            throw new NotImplementedException();
        }

        public Task<IADOrganizationalUnit?> GetParent()
        {
            throw new NotImplementedException();
        }

        public bool Invoke(string method, object?[]? args = null)
        {
            throw new NotImplementedException();
        }

        public bool IsNewDirectoryEntry(IADOrganizationalUnit ou)
        {
            throw new NotImplementedException();
        }

        public bool MoveTo(IADOrganizationalUnit parentOUToMoveTo)
        {
            throw new NotImplementedException();
        }

        public Task Parse(DirectoryEntry result, IActiveDirectoryContext directory)
        {
            throw new NotImplementedException();
        }

        public Task Parse(SearchResult result, IActiveDirectoryContext directory)
        {
            throw new NotImplementedException();
        }

        public bool Rename(string newName)
        {
            throw new NotImplementedException();
        }

        public void SetCustomProperty(string propertyName, object? value)
        {
            throw new NotImplementedException();
        }
    }
}
