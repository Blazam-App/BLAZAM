namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// A searcher class for OU objects in an <see cref="IActiveDirectoryContext"/>
    /// </summary>
    public interface IADOUSearcher
    {
        /// <summary>
        /// Finds all users within a specified organizational unit and its sub-OUs by distinguished name.
        /// </summary>
        /// <param name="searchTerm">The distinguished name (DN) of the organizational unit to search within.</param>
        /// <returns>A list of <see cref="IADUser"/> objects found within the specified OU and its children.</returns>
        List<IADUser> FindSubUsersByDN(string searchTerm);

        /// <summary>
        /// Finds all child organizational units within a specified OU by distinguished name.
        /// </summary>
        /// <param name="searchTerm">The distinguished name (DN) of the parent organizational unit.</param>
        /// <returns>A list of <see cref="IADOrganizationalUnit"/> objects that are children of the specified OU.</returns>
        List<IADOrganizationalUnit> FindSubOusByDN(string searchTerm);

        /// <summary>
        /// Finds a specific organizational unit by its distinguished name.
        /// </summary>
        /// <param name="searchTerm">The distinguished name (DN) of the organizational unit to find.</param>
        /// <returns>The <see cref="IADOrganizationalUnit"/> matching the DN, or null if not found.</returns>
        IADOrganizationalUnit? FindOuByDN(string searchTerm);

        /// <summary>
        /// Finds a specific organizational unit by its distinguished name asynchronously.
        /// </summary>
        /// <param name="searchTerm">The distinguished name (DN) of the organizational unit to find.</param>
        /// <returns>The <see cref="IADOrganizationalUnit"/>A Task whose result has a matching DN, or null if not found.</returns>
        Task<IADOrganizationalUnit?> FindOuByDNAsync(string searchTerm);

        /// <summary>
        /// Gets the root organizational unit configured for the application.
        /// </summary>
        /// <returns>The application's root <see cref="IADOrganizationalUnit"/>.</returns>
        IADOrganizationalUnit GetApplicationRootOU();

        /// <summary>
        /// Finds organizational units matching a search string.
        /// </summary>
        /// <param name="searchTerm">The search string to match against OU names or properties.</param>
        /// <returns>A list of <see cref="IADOrganizationalUnit"/> objects matching the search criteria.</returns>
        List<IADOrganizationalUnit> FindOuByString(string searchTerm);

        /// <summary>
        /// Asynchronously finds organizational units matching a search string.
        /// </summary>
        /// <param name="searchTerm">The search string to match against OU names or properties.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of <see cref="IADOrganizationalUnit"/> objects matching the search criteria.</returns>
        Task<List<IADOrganizationalUnit>> FindOuByStringAsync(string searchTerm);

        /// <summary>
        /// Finds all computers within a specified organizational unit and its sub-OUs by distinguished name.
        /// </summary>
        /// <param name="searchBaseDN">The distinguished name (DN) of the organizational unit to search within.</param>
        /// <returns>A list of <see cref="IADComputer"/> objects found within the specified OU and its children.</returns>
        List<IADComputer> FindSubComputerByDN(string searchBaseDN);

        /// <summary>
        /// Finds all groups within a specified organizational unit and its sub-OUs by distinguished name.
        /// </summary>
        /// <param name="searchBaseDN">The distinguished name (DN) of the organizational unit to search within.</param>
        /// <returns>A list of <see cref="IADGroup"/> objects found within the specified OU and its children.</returns>
        List<IADGroup> FindSubGroupsByDN(string searchBaseDN);

        /// <summary>
        /// Finds organizational units that were created recently within a specified time period.
        /// </summary>
        /// <param name="maxAgeInDays">The maximum age in days for OUs to be considered "new". Default is 14 days.</param>
        /// <returns>A list of <see cref="IADOrganizationalUnit"/> objects created within the specified time period.</returns>
        List<IADOrganizationalUnit> FindNewOUs(int maxAgeInDays = 14);

        /// <summary>
        /// Asynchronously finds organizational units that were created recently within a specified time period.
        /// </summary>
        /// <param name="maxAgeInDays">The maximum age in days for OUs to be considered "new". Default is 14 days.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of <see cref="IADOrganizationalUnit"/> objects created within the specified time period.</returns>
        Task<List<IADOrganizationalUnit>> FindNewOUsAsync(int maxAgeInDays = 14);
    }
}