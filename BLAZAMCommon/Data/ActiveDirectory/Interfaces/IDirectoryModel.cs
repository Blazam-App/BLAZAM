using BLAZAM.Common.Data.ActiveDirectory.Models;
using BLAZAM.Common.Helpers;
using BLAZAM.Common.Models.Database;
using Microsoft.EntityFrameworkCore;
using System.DirectoryServices;

namespace BLAZAM.Common.Data.ActiveDirectory.Interfaces
{
    public interface IDirectoryModel : IDisposable
    {
        string? SamAccountName { get; set; }
        string? CanonicalName { get; set; }
        string? DN { get; set; }
        DateTime? Created { get; set; }
        DateTime? LastChanged { get; set; }
        byte[]? SID { get; set; }
        List<string>? Classes { get; set; }
        bool CanRead { get; }
        ActiveDirectoryObjectType ObjectType { get; }
        bool CanMove { get; }
        bool CanRename { get; }
        bool CanCreate { get; }
        bool CanDelete { get; }
        DirectoryEntry DirectoryEntry { get; set; }
        string? ADSPath { get; set; }
        bool CanEdit { get; }
        string? OU { get; }
        bool HasUnsavedChanges { get; }
        bool NewEntry { get; set; }
        /// <summary>
        /// Returns the relative path to reach
        /// the search result page for this 
        /// entry.
        /// </summary>
        string SearchUri { get; }
        AppEvent? OnModelCommited { get; set; }
        AppEvent? OnModelChanged { get; set; }
        Dictionary<string, object> NewEntryProperties { get; set; }
        IADOrganizationalUnit? LastKnownParent { get; }
        bool IsDeleted { get; }
        List<AuditChangeLog> Changes { get; }
        AppEvent<IDirectoryModel> OnDirectoryModelRenamed { get; set; }

        DirectoryChangeResult CommitChanges();
        Task<DirectoryChangeResult> CommitChangesAsync();
        void DiscardChanges();
        bool CanEditField(ActiveDirectoryField field);
        bool CanReadField(ActiveDirectoryField field);

        new void Dispose();
        Task Parse(DirectoryEntry result, IActiveDirectory directory);
        Task Parse(SearchResult result, IActiveDirectory directory);
        bool MoveTo(IADOrganizationalUnit parentOUToMoveTo);
        Task<IADOrganizationalUnit?> GetParent();
        bool IsNewDirectoryEntry(IADOrganizationalUnit ou);
        bool Invoke(string method, object?[]? args = null);
        void Delete();
        string? ToString();
        bool Rename(string newName);
    }
}
