using System.Collections;
using System.DirectoryServices.Protocols;
using BLAZAM.ActiveDirectory.Interfaces;

namespace BLAZAM.ActiveDirectory.Adapters
{
    /// <summary>
    /// A collection of directory entries that uses System.DirectoryServices.Protocols
    /// to interact with the LDAP server.
    /// </summary>
    public class LdapDirectoryEntries : IDirectoryEntries
    {
        private readonly string _parentDn;
        private readonly IActiveDirectoryContext _directory;

        public LdapDirectoryEntries(string parentDn, IActiveDirectoryContext directory)
        {
            _parentDn = parentDn;
            _directory = directory;
        }



        /// <summary>
        /// Gets an enumerator that iterates through the child entries.
        /// </summary>
        /// <returns>An IEnumerator for the collection of child IDirectoryEntry objects.</returns>
        public IEnumerator GetEnumerator()
        {
            using var connection = _directory.GetConnection();
            if (connection == null) yield break;

            var request = new SearchRequest(
                _parentDn,
                "(objectClass=*)", // Filter to find all objects
                System.DirectoryServices.Protocols.SearchScope.OneLevel, // Search only the immediate children
                "*" // Request all attributes
            );

            var pageRequestControl = new PageResultRequestControl(1000);
            request.Controls.Add(pageRequestControl);

            while (true)
            {
                var response = (SearchResponse)connection.SendRequest(request);

                foreach (SearchResultEntry entry in response.Entries)
                {
                    yield return new LdapDirectoryEntry(entry, _directory);
                }

                var pageResponseControl = response.Controls
                    .OfType<PageResultResponseControl>()
                    .FirstOrDefault();

                if (pageResponseControl == null || pageResponseControl.Cookie.Length == 0)
                {
                    break;
                }

                pageRequestControl.Cookie = pageResponseControl.Cookie;
            }
        }

        /// <summary>
        /// Adds a new entry to the directory under the current parent DN.
        /// </summary>
        /// <param name="name">The RDN of the new object (e.g., "CN=New User").</param>
        /// <param name="schemaClassName">The primary object class of the new entry (e.g., "user").</param>
        /// <returns>The newly created directory entry.</returns>
        public IDirectoryEntry Add(string name, string schemaClassName)
        {
            string newEntryDn = name + "," + _parentDn;
            var request = new AddRequest(newEntryDn, schemaClassName);

            using var connection = _directory.GetConnection();
            connection.SendRequest(request);

            return new LdapDirectoryEntry(newEntryDn, _directory);
        }

        /// <summary>
        /// Finds a child entry by its name (RDN value).
        /// </summary>
        /// <param name="name">The name of the entry to find (e.g., "CN=John Doe").</param>
        /// <returns>The found directory entry.</returns>
        public IDirectoryEntry Find(string name)
        {
            return Find(name, null);
        }

        /// <summary>
        /// Finds a child entry by its name and optionally by its schema class.
        /// </summary>
        /// <param name="name">The name of the entry to find (e.g., "CN=John Doe").</param>
        /// <param name="schemaClassName">The schema class to filter by (optional).</param>
        /// <returns>The found directory entry.</returns>
        public IDirectoryEntry Find(string name, string schemaClassName)
        {
            string filter = "(&(objectClass=*)(|(cn=" + name + ")(ou=" + name + ")))";
            if (!string.IsNullOrEmpty(schemaClassName))
                filter = "(&(objectClass=" + schemaClassName + ")(|(cn=" + name + ")(ou=" + name + ")))";


            using var connection = _directory.GetConnection();
            var request = new SearchRequest(_parentDn, filter, System.DirectoryServices.Protocols.SearchScope.OneLevel, null);
            var response = (SearchResponse)connection.SendRequest(request);

            if (response.Entries.Count > 0)
            {
                return new LdapDirectoryEntry(response.Entries[0].DistinguishedName, _directory);
            }
            return null;
        }

        /// <summary>
        /// Removes a child entry from the directory.
        /// </summary>
        /// <param name="entry">The directory entry to remove.</param>
        public void Remove(IDirectoryEntry entry)
        {
            var request = new DeleteRequest(entry.DN);
            using var connection = _directory.GetConnection();
            connection.SendRequest(request);
        }

    }

}
