namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// A searcher class for OU objects in an <see cref="IActiveDirectoryContext"/>
    /// </summary>
    public interface IADOUSearcher
    {
        List<IADUser> FindSubUsersByDN(string searchTerm);
        List<IADOrganizationalUnit> FindSubOusByDN(string searchTerm);
        IADOrganizationalUnit? FindOuByDN(string searchTerm);
        IADOrganizationalUnit GetApplicationRootOU();
        List<IADOrganizationalUnit> FindOuByString(string searchTerm);
        Task<List<IADOrganizationalUnit>> FindOuByStringAsync(string searchTerm);
        List<IADComputer> FindSubComputerByDN(string searchBaseDN);
        List<IADGroup> FindSubGroupsByDN(string searchBaseDN);
        List<IADOrganizationalUnit> FindNewOUs(int maxAgeInDays = 14);
        Task<List<IADOrganizationalUnit>> FindNewOUsAsync(int maxAgeInDays = 14);
    }
}