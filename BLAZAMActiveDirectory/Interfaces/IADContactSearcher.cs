namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// Provides methods for searching and retrieving Active Directory contacts based on various criteria.
    /// </summary>
    public interface IADContactSearcher
    {
        /// <summary>
        /// Finds contacts that have been modified within the specified time period.
        /// </summary>
        /// <param name="ignoreDisabledUsers">When true, excludes disabled user accounts from the results. Default is true.</param>
        /// <param name="daysBackToSearch">The number of days to search backwards from the current date. Default is 90.</param>
        /// <returns>A list of contacts that have been changed within the specified time period.</returns>
        List<IADContact> FindChangedContacts(bool ignoreDisabledUsers = true, int daysBackToSearch = 90);

        /// <summary>
        /// Asynchronously finds contacts that have been modified recently.
        /// </summary>
        /// <param name="ignoreDisabledUsers">When true, excludes disabled user accounts from the results. Default is true.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of changed contacts.</returns>
        Task<List<IADContact>> FindChangedContactsAsync(bool ignoreDisabledUsers = true);

        /// <summary>
        /// Finds a contact by searching within a specific container name.
        /// </summary>
        /// <param name="searchTerm">The container name to search for.</param>
        /// <param name="exactMatch">When true, requires an exact match of the container name. Default is false.</param>
        /// <returns>The matching contact, or null if no contact is found.</returns>
        IADContact? FindContactsByContainerName(string? searchTerm, bool exactMatch = false);

        /// <summary>
        /// Finds a contact by its GUID (byte array format).
        /// </summary>
        /// <param name="guid">The GUID of the contact as a byte array.</param>
        /// <returns>The matching contact, or null if no contact is found.</returns>
        IADContact? FindContactsByGUID(byte[]? guid);

        /// <summary>
        /// Finds a contact by its GUID.
        /// </summary>
        /// <param name="guid">The GUID of the contact.</param>
        /// <returns>The matching contact, or null if no contact is found.</returns>
        IADContact? FindContactsByGUID(Guid? guid);

        /// <summary>
        /// Searches for contacts using a string search term across contact attributes.
        /// </summary>
        /// <param name="searchTerm">The search term to match against contact attributes.</param>
        /// <param name="exactMatch">When true, requires an exact match of the search term. Default is false.</param>
        /// <returns>A list of contacts matching the search criteria.</returns>
        List<IADContact> FindContactsByString(string? searchTerm, bool exactMatch = false);

        /// <summary>
        /// Asynchronously searches for contacts using a string search term across contact attributes.
        /// </summary>
        /// <param name="searchTerm">The search term to match against contact attributes.</param>
        /// <param name="exactMatch">When true, requires an exact match of the search term. Default is false.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of matching contacts.</returns>
        Task<List<IADContact>> FindContactsByStringAsync(string? searchTerm, bool exactMatch = false);

        /// <summary>
        /// Finds all contacts that have expired based on their account expiration date.
        /// </summary>
        /// <returns>A list of expired contacts.</returns>
        List<IADContact> FindExpiredContacts();

        /// <summary>
        /// Finds contacts that were recently created within the specified maximum age.
        /// </summary>
        /// <param name="maxAgeInDays">The maximum age in days for a contact to be considered new. Default is 14.</param>
        /// <param name="ignoreDisabledUsers">When true, excludes disabled user accounts from the results. Default is true.</param>
        /// <returns>A list of newly created contacts.</returns>
        List<IADContact> FindNewContacts(int maxAgeInDays = 14, bool ignoreDisabledUsers = true);

        /// <summary>
        /// Asynchronously finds contacts that were recently created within the specified maximum age.
        /// </summary>
        /// <param name="maxAgeInDays">The maximum age in days for a contact to be considered new. Default is 14.</param>
        /// <param name="ignoreDisabledUsers">When true, excludes disabled user accounts from the results. Default is true.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of newly created contacts.</returns>
        Task<List<IADContact>> FindNewContactsAsync(int maxAgeInDays = 14, bool ignoreDisabledUsers = true);
    }
}