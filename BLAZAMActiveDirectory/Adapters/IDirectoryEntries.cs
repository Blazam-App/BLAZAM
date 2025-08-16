
using BLAZAM.ActiveDirectory.Interfaces;
using System.Collections;

namespace BLAZAM.ActiveDirectory.Adapters
{
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
#pragma warning restore CA1416 // Validate platform compatibility

}
