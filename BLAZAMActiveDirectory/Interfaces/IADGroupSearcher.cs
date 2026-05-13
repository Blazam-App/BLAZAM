namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// A searcher class for group objects in an <see cref="IActiveDirectoryContext"/>
    /// </summary>
    public interface IADGroupSearcher
    {
        /// <summary>
        /// Finds a group by its Security Identifier (SID) in byte array format
        /// </summary>
        /// <param name="groupSID">The SID as a byte array</param>
        /// <returns>The matching group or null if not found</returns>
        IADGroup? FindGroupBySID(byte[] groupSID);
        
        /// <summary>
        /// Finds a group by its Security Identifier (SID) in string format
        /// </summary>
        /// <param name="groupSID">The SID as a string (e.g., "S-1-5-21-...")</param>
        /// <returns>The matching group or null if not found</returns>
        IADGroup? FindGroupBySID(string groupSID);
        
        /// <summary>
        /// Searches for groups matching a search term
        /// </summary>
        /// <param name="searchTerm">The search string to match against group properties</param>
        /// <param name="exactMatch">If true, requires an exact match; otherwise performs a partial match</param>
        /// <returns>A list of matching groups</returns>
        List<IADGroup> FindGroupByString(string searchTerm, bool exactMatch = false);
        
        /// <summary>
        /// Asynchronously searches for groups matching a search term
        /// </summary>
        /// <param name="searchTerm">The search string to match against group properties</param>
        /// <param name="exactMatch">If true, requires an exact match; otherwise performs a partial match</param>
        /// <returns>A task containing a list of matching groups</returns>
        Task<List<IADGroup>> FindGroupByStringAsync(string searchTerm, bool exactMatch = false);
        
        /// <summary>
        /// Finds groups by their Distinguished Names
        /// </summary>
        /// <param name="list">A list of Distinguished Names to search for</param>
        /// <returns>A list of matching groups</returns>
        List<IADGroup> FindGroupsByDN(List<string>? list);
        
        /// <summary>
        /// Gets all direct members (users and groups) of a group
        /// </summary>
        /// <param name="group">The group to retrieve members from</param>
        /// <returns>A list of groups that are direct members</returns>
        List<IADGroup> GetGroupMembers(IADGroup group);
        
        /// <summary>
        /// Gets only the direct user members of a group
        /// </summary>
        /// <param name="group">The group to retrieve user members from</param>
        /// <param name="ignoreDisabledUsers">If true, excludes disabled user accounts from the results</param>
        /// <returns>A list of users that are direct members</returns>
        List<IADUser> GetDirectUserMembers(IADGroup group, bool ignoreDisabledUsers = true);
        
        /// <summary>
        /// Checks if a user or group is a nested member of the specified group
        /// </summary>
        /// <param name="group">The group to check membership in</param>
        /// <param name="userOrGroup">The user or group to check for membership</param>
        /// <param name="v">Purpose unclear - consider renaming this parameter</param>
        /// <param name="ignoreDisabledUsers">If true, ignores disabled users during nested membership checks</param>
        /// <returns>True if the user/group is a nested member; otherwise false</returns>
        bool IsANestedMemberOf(IADGroup? group, IGroupableDirectoryAdapter? userOrGroup, bool v, bool ignoreDisabledUsers = true);
        
        /// <summary>
        /// Asynchronously finds groups created within a specified time period
        /// </summary>
        /// <param name="maxAgeInDays">The maximum age in days (default: 14 days)</param>
        /// <returns>A task containing a list of newly created groups</returns>
        Task<List<IADGroup>> FindNewGroupsAsync(int maxAgeInDays = 14);
        
        /// <summary>
        /// Finds groups created within a specified time period
        /// </summary>
        /// <param name="maxAgeInDays">The maximum age in days (default: 14 days)</param>
        /// <returns>A list of newly created groups, or null if none found</returns>
        List<IADGroup>? FindNewGroups(int maxAgeInDays = 14);
        
        /// <summary>
        /// Recursively retrieves all nested members (users and groups) of a group
        /// </summary>
        /// <param name="group">The group to retrieve all nested members from</param>
        /// <returns>A list of all nested members (both users and groups), or null if none found</returns>
        List<IGroupableDirectoryAdapter>? GetAllNestedMembers(IADGroup group);
    }
}