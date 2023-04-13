
using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.Common.Data;
using BLAZAM.Database.Models;
using System.DirectoryServices;

namespace BLAZAM.ActiveDirectory.Interfaces
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

        /// <summary>
        /// The type of Active Directory entry that this is
        /// </summary>
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

        /// <summary>
        /// Indicates whether the current web application user
        /// can create this <see cref="ActiveDirectoryObjectType"/>
        /// </summary>
        /// <remarks>
        /// Checks against permissions set in database. Always
        /// true for super admins.
        /// </remarks>
        bool CanCreate { get; }

        /// <summary>
        /// Indicates whether the current web application user
        /// can delete this directory entry
        /// </summary>
        /// <remarks>
        /// Checks against permissions set in database. Always
        /// true for super admins.
        /// </remarks>
        bool CanDelete { get; }

        /// <summary>
        /// The .NET underlying object for this entry
        /// </summary>
        DirectoryEntry? DirectoryEntry { get; set; }

        /// <summary>
        /// The full Active Directory Services path including LDAP server name
        /// </summary>
        string? ADSPath { get; set; }

        /// <summary>
        /// Indicates whether the current web application user
        /// can edit any fields for this directory entry
        /// </summary>
        /// <remarks>
        /// Checks against permissions set in database. Always
        /// true for super admins.
        /// </remarks>
        bool CanEdit { get; }

        /// <summary>
        /// Indicates whether the current web application user
        /// can read any custom fields for this directory entry
        /// </summary>
        /// <remarks>
        /// Checks against permissions set in database. Always
        /// true for super admins.
        /// </remarks>
        bool CanReadAnyCustomFields { get; }


        /// <summary>
        /// The parent OU distinguished name as a string
        /// </summary>
        string? OU { get; }

        /// <summary>
        /// Indicates whether there have been uncommitted
        /// changes to this entry
        /// </summary>
        /// <remarks>
        /// Compares the current state to the initial state
        /// </remarks>
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

        /// <summary>
        /// Called when pending changes to this entry are commited
        /// </summary>
        AppEvent? OnModelCommited { get; set; }

        /// <summary>
        /// Called when any changes occur to this entry
        /// </summary>
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

        /// <summary>
        /// Indicates this entry is in the Active Directory Recycle Bin
        /// </summary>
        bool IsDeleted { get; }

        /// <summary>
        /// A list of changelogs made to this entry for auditting purposes
        /// </summary>
        /// <remarks>
        /// Must be collected prior to executing <see cref="CommitChanges"/>
        /// </remarks>
        List<AuditChangeLog> Changes { get; }

        /// <summary>
        /// Called when this entry is renamed
        /// </summary>
        AppEvent<IDirectoryEntryAdapter>? OnDirectoryModelRenamed { get; set; }
        AppEvent? OnModelDeleted { get; set; }
        IActiveDirectoryContext Directory { get; }

        /// <summary>
        /// Sends all staged changes to the Active Directory server
        /// </summary>
        /// <returns></returns>
        DirectoryChangeResult CommitChanges();
        /// <summary>
        /// Sends all staged changes to the Active Directory server asynchronously
        /// </summary>
        /// <returns></returns>
        Task<DirectoryChangeResult> CommitChangesAsync();

        /// <summary>
        /// Resets the current entry state to it's inital state
        /// </summary>
        void DiscardChanges();

        /// <summary>
        /// Indicates whether the current web application user
        /// can modify the specified field
        /// </summary>
        /// <remarks>
        /// Checks against permissions set in database. Always
        /// true for super admins.
        /// </remarks>
        /// <param name="field">The field to test</param>
        /// <returns>True if the current user can modify the field, otherwise false</returns>
        bool CanEditField(IActiveDirectoryField field);

        /// <summary>
        /// Indicates whether the current web application user
        /// can read the specified field
        /// </summary>
        /// <remarks>
        /// Checks against permissions set in database. Always
        /// true for super admins.
        /// </remarks>
        /// <param name="field">The field to test</param>
        /// <returns>True if the current user can read the field, otherwise false</returns>
        bool CanReadField(IActiveDirectoryField field);


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
        void EnsureDirectoryEntry();
        T? GetCustomProperty<T>(string propertyName);
        DateTime? GetDateTimeProperty(string propertyName);
        void SetCustomProperty(string propertyName, object? value);
    }
}
