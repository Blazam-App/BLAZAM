using BLAZAM.Common.Data.ActiveDirectory.Models;

namespace BLAZAM.Common.Data.ActiveDirectory.Interfaces
{
    public interface IADGroupSearcher
    {
        IADGroup? FindGroupBySID(byte[] groupSID);
        IADGroup? FindGroupBySID(string groupSID);
        List<IADGroup> FindGroupByString(string searchTerm, bool exactMatch = false);
        Task<List<IADGroup>> FindGroupByStringAsync(string searchTerm, bool exactMatch = false);
        List<IADGroup> FindGroupsByDN(List<string>? list);
        List<IADGroup> GetGroupMembers(IADGroup group);
        List<IADUser> GetDirectUserMembers(IADGroup group, bool ignoreDisabledUsers = true);
        bool IsAMemberOf(IADGroup? group, IGroupableDirectoryAdapter? user, bool v, bool ignoreDisabledUsers = true);
        Task<List<IADGroup>> FindNewGroupsAsync();
        List<IADGroup>? FindNewGroups();
        // List<IGroupableDirectoryModel> GetMembers (IADGroup aDGroup);
    }
}