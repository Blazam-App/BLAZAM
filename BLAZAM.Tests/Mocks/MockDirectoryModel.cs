using BLAZAM.Common.Data.ActiveDirectory;
using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.ActiveDirectory.Models;
using BLAZAM.Common.Models.Database;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Tests.Mocks
{
    internal class MockDirectoryModel : IDirectoryModel
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

        public List<DirectoryModelChange> Changes => throw new NotImplementedException();

        public AppEvent<IDirectoryModel> OnDirectoryModelRenamed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool CanEditField(ActiveDirectoryField field)
        {
            throw new NotImplementedException();
        }

        public bool CanReadField(ActiveDirectoryField field)
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

        public Task Parse(DirectoryEntry result, IActiveDirectory directory)
        {
            throw new NotImplementedException();
        }

        public Task Parse(SearchResult result, IActiveDirectory directory)
        {
            throw new NotImplementedException();
        }

        public bool Rename(string newName)
        {
            throw new NotImplementedException();
        }
    }
}
