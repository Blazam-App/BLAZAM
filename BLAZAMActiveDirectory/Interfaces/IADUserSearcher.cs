
namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// A searcher class for user objects in an <see cref="IActiveDirectoryContext"/>
    /// </summary>
    public interface IADUserSearcher
    {
        List<IADUser> FindUsersByString(string? searchTerm, bool? ignoreDisabledUsers = true, bool exactMatch = false);
        IADUser? FindUsersByContainerName(string? searchTerm, bool? ignoreDisabledUsers = true, bool exactMatch = true);
        Task<List<IADUser>> FindUsersByStringAsync(string? searchTerm, bool? ignoreDisabledUsers = true, bool exactMatch = false);
        Task<List<IADUser>> FindLockedOutUsersAsync(bool? ignoreDisabledUsers = true);
        List<IADUser>? FindLockedOutUsers(bool? ignoreDisabledUsers = true);
        IADUser? FindUserBySID(string? sid);
        IADUser? FindUserByUsername(string? searchTerm, bool? ignoreDisabledUsers = true);
        List<IADUser>? FindNewUsers(int maxAgeInDays = 14, bool? ignoreDisabledUsers = true);
        Task<List<IADUser>> FindNewUsersAsync(int maxAgeInDays = 14, bool? ignoreDisabledUsers = true);
        List<IADUser>? FindChangedUsers(bool? ignoreDisabledUsers = true, int daysBackToSearch = 90);
        Task<List<IADUser>> FindChangedUsersAsync(bool? ignoreDisabledUsers = true);
        Task<List<IADUser>> FindChangedPasswordUsersAsync(bool? ignoreDisabledUsers = true);
        List<IADUser>? FindChangedPasswordUsers(bool? ignoreDisabledUsers = true);
    }
}