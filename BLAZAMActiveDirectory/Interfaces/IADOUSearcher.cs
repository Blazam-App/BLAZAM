namespace BLAZAM.ActiveDirectory.Interfaces
{
    public interface IADOUSearcher
    {
        List<IADUser> FindSubUsersByDN(string searchTerm);
        List<IADOrganizationalUnit> FindSubOusByDN(string searchTerm);
        IADOrganizationalUnit? FindOuByDN(string searchTerm);
        Task<IADOrganizationalUnit> GetApplicationRootOU();
        List<IADOrganizationalUnit> FindOuByString(string searchTerm);
        Task<List<IADOrganizationalUnit>> FindOuByStringAsync(string searchTerm);
        List<IADComputer> FindSubComputerByDN(string searchBaseDN);
        List<IADGroup> FindSubGroupsByDN(string searchBaseDN);
        List<IADOrganizationalUnit> FindNewOUs(int maxAgeInDays = 14);
        Task<List<IADOrganizationalUnit>> FindNewOUsAsync(int maxAgeInDays = 14);
    }
}