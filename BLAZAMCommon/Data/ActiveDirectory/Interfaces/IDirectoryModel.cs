using BLAZAM.Common.Data.ActiveDirectory.Models;
using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Models.Database;
using System.DirectoryServices;

namespace BLAZAM.Common.Data.ActiveDirectory.Interfaces
{
    public interface IDirectoryEntryAdapter : IDisposable
    {
        string? SamAccountName { get; set; }
        string? CanonicalName { get; set; }
        string? DN { get; set; }

        /// <summary>
        /// The date and time in UTC that this entry was created
        /// </summary>
        DateTime? Created { get; set; }

        /// <summary>
        /// The date and time in UTC that this entry was last modified
        /// </summary>
        /// <remarks>
        /// Many AD functions count as updating a user, such as logging in...
        /// </remarks>
        DateTime? LastChanged { get; set; }
        byte[]? SID { get; set; }
        
        /// <summary>
        /// The objectClass value
        /// </summary>
        List<string>? Classes { get; set; }

        /// <summary>
        /// Indicates whether the current web application user
        /// has read access to this directory entry
        /// </summary>
        /// <remarks>
        /// Checks against permissions set in database. Always
        /// true for super admins.
        /// </remarks>
        bool CanRead { get; }
        ActiveDirectoryObjectType ObjectType { get; }

        /// <summary>
        /// Indicates whether the current web application user
        /// has move permission for this directory entry
        /// </summary>
        /// <remarks>
        /// Checks against permissions set in database. Always
        /// true for super admins.
        /// </remarks>
        bool CanMove { get; }

        /// <summary>
        /// Indicates whether the current web application user
        /// has read access to this directory entry
        /// </summary>
        /// <remarks>
        /// Checks against permissions set in database. Always
        /// true for super admins.
        /// </remarks>
        bool CanRename { get; }
        bool CanCreate { get; }
        bool CanDelete { get; }
        DirectoryEntry? DirectoryEntry { get; set; }
        string? ADSPath { get; set; }
        bool CanEdit { get; }

        /// <summary>
        /// The parent OU distinguished name as a string
        /// </summary>
        string? OU { get; }
        bool HasUnsavedChanges { get; }

        /// <summary>
        /// Set this to true to indicate that this entry is a new
        /// entry.
        /// </summary>
        bool NewEntry { get; set; }

        /// <summary>
        /// Returns the relative path to reach
        /// the search result page for this 
        /// entry.
        /// </summary>
        string SearchUri { get; }
        AppEvent? OnModelCommited { get; set; }
        AppEvent? OnModelChanged { get; set; }

        /// <summary>
        /// If <see cref="NewEntry"/> is true, property changes will be
        /// staged in this property.
        /// </summary>
        Dictionary<string, object> NewEntryProperties { get; set; }

        /// <summary>
        /// If this object is deleted, this was the last <see cref="IADOrganizationalUnit"/> it was under
        /// </summary>
        IADOrganizationalUnit? LastKnownParent { get; }

        bool IsDeleted { get; }


        List<AuditChangeLog> Changes { get; }
        AppEvent<IDirectoryEntryAdapter>? OnDirectoryModelRenamed { get; set; }

        DirectoryChangeResult CommitChanges();
        Task<DirectoryChangeResult> CommitChangesAsync();
        void DiscardChanges();
        bool CanEditField(ActiveDirectoryField field);
        bool CanReadField(ActiveDirectoryField field);

        new void Dispose();

        /// <summary>
        /// Converts a raw <see cref="DirectoryEntry"/> into 
        /// an application <see cref="IDirectoryEntryAdapter"/>
        /// object.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        Task Parse(DirectoryEntry result, IActiveDirectoryContext directory);
        Task Parse(SearchResult result, IActiveDirectoryContext directory);
        bool MoveTo(IADOrganizationalUnit parentOUToMoveTo);
        Task<IADOrganizationalUnit?> GetParent();

        /// <summary>
        /// Wrapper method for the base <see cref="DirectoryEntry"/>
        /// Invoke method.
        /// </summary>
        /// <remarks>
        /// Useful for setting passwords and managing group
        /// memberships.
        /// </remarks>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        bool Invoke(string method, object?[]? args = null);
        void Delete();
        string? ToString();
        bool Rename(string newName);
    }
}
