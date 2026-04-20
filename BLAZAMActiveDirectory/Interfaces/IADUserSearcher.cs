    namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// A searcher class for user objects in an <see cref="IActiveDirectoryContext"/>
    /// </summary>
    public interface IADUserSearcher
    {
        /// <summary>
        /// Finds users matching the specified search term
        /// </summary>
        /// <param name="searchTerm">The search string to match against user properties</param>
        /// <param name="ignoreDisabledUsers">If true, excludes disabled user accounts from results</param>
        /// <param name="exactMatch">If true, requires an exact match; otherwise performs a partial match</param>
        /// <returns>A list of users matching the search criteria</returns>
        List<IADUser> FindUsersByString(string? searchTerm, bool ignoreDisabledUsers = true, bool exactMatch = false);
        
        /// <summary>
        /// Finds users by their container name in Active Directory
        /// </summary>
        /// <param name="searchTerm">The container name to search for</param>
        /// <param name="ignoreDisabledUsers">If true, excludes disabled user accounts from results</param>
        /// <param name="exactMatch">If true, requires an exact match; otherwise performs a partial match</param>
        /// <returns>The user matching the container name, or null if not found</returns>
        IADUser? FindUsersByContainerName(string? searchTerm, bool ignoreDisabledUsers = true, bool exactMatch = false);
        
        /// <summary>
        /// Asynchronously finds users matching the specified search term
        /// </summary>
        /// <param name="searchTerm">The search string to match against user properties</param>
        /// <param name="ignoreDisabledUsers">If true, excludes disabled user accounts from results</param>
        /// <param name="exactMatch">If true, requires an exact match; otherwise performs a partial match</param>
        /// <returns>A task representing the asynchronous operation, containing a list of matching users</returns>
        Task<List<IADUser>> FindUsersByStringAsync(string? searchTerm, bool ignoreDisabledUsers = true, bool exactMatch = false);
        
        /// <summary>
        /// Asynchronously finds all locked out user accounts
        /// </summary>
        /// <param name="ignoreDisabledUsers">If true, excludes disabled user accounts from results</param>
        /// <returns>A task representing the asynchronous operation, containing a list of locked out users</returns>
        Task<List<IADUser>> FindLockedOutUsersAsync(bool ignoreDisabledUsers = true);
        
        /// <summary>
        /// Finds all locked out user accounts
        /// </summary>
        /// <param name="ignoreDisabledUsers">If true, excludes disabled user accounts from results</param>
        /// <returns>A list of locked out users, or null if the operation fails</returns>
        List<IADUser>? FindLockedOutUsers(bool ignoreDisabledUsers = true);
        
        /// <summary>
        /// Finds a user by their Security Identifier (SID)
        /// </summary>
        /// <param name="sid">The SID of the user to find</param>
        /// <returns>The user with the specified SID, or null if not found</returns>
        IADUser? FindUserBySID(string? sid);
        
        /// <summary>
        /// Finds a user by their username
        /// </summary>
        /// <param name="searchTerm">The username to search for</param>
        /// <param name="ignoreDisabledUsers">If true, excludes disabled user accounts from results</param>
        /// <param name="exactMatch">If true, requires an exact match; otherwise performs a partial match</param>
        /// <returns>The user with the specified username, or null if not found</returns>
        IADUser? FindUserByUsername(string? searchTerm, bool ignoreDisabledUsers = true, bool exactMatch = false);
        
        /// <summary>
        /// Finds newly created user accounts within a specified time period
        /// </summary>
        /// <param name="maxAgeInDays">The maximum age in days for a user to be considered new</param>
        /// <param name="ignoreDisabledUsers">If true, excludes disabled user accounts from results</param>
        /// <returns>A list of new users, or null if the operation fails</returns>
        List<IADUser>? FindNewUsers(int maxAgeInDays = 14, bool ignoreDisabledUsers = true);
        
        /// <summary>
        /// Asynchronously finds newly created user accounts within a specified time period
        /// </summary>
        /// <param name="maxAgeInDays">The maximum age in days for a user to be considered new</param>
        /// <param name="ignoreDisabledUsers">If true, excludes disabled user accounts from results</param>
        /// <returns>A task representing the asynchronous operation, containing a list of new users</returns>
        Task<List<IADUser>> FindNewUsersAsync(int maxAgeInDays = 14, bool ignoreDisabledUsers = true);
        
        /// <summary>
        /// Finds user accounts that have been modified within a specified time period
        /// </summary>
        /// <param name="ignoreDisabledUsers">If true, excludes disabled user accounts from results</param>
        /// <param name="daysBackToSearch">The number of days to search back for changes</param>
        /// <returns>A list of modified users, or null if the operation fails</returns>
        List<IADUser>? FindChangedUsers(bool ignoreDisabledUsers = true, int daysBackToSearch = 90);
        
        /// <summary>
        /// Asynchronously finds user accounts that have been modified
        /// </summary>
        /// <param name="ignoreDisabledUsers">If true, excludes disabled user accounts from results</param>
        /// <returns>A task representing the asynchronous operation, containing a list of modified users</returns>
        Task<List<IADUser>> FindChangedUsersAsync(bool ignoreDisabledUsers = true);
        
        /// <summary>
        /// Asynchronously finds users whose passwords have been changed within a specified time period
        /// </summary>
        /// <param name="maxAgeInDays">The maximum age in days for a password change to be considered recent</param>
        /// <param name="ignoreDisabledUsers">If true, excludes disabled user accounts from results</param>
        /// <returns>A task representing the asynchronous operation, containing a list of users with recently changed passwords</returns>
        Task<List<IADUser>> FindChangedPasswordUsersAsync(int maxAgeInDays = 90, bool ignoreDisabledUsers = true);
        
        /// <summary>
        /// Finds users whose passwords have been changed within a specified time period
        /// </summary>
        /// <param name="maxAgeInDays">The maximum age in days for a password change to be considered recent</param>
        /// <param name="ignoreDisabledUsers">If true, excludes disabled user accounts from results</param>
        /// <returns>A list of users with recently changed passwords, or null if the operation fails</returns>
        List<IADUser>? FindChangedPasswordUsers(int maxAgeInDays = 90, bool ignoreDisabledUsers = true);
        
        /// <summary>
        /// Finds user accounts that have expired
        /// </summary>
        /// <param name="ignoreDisabledUsers">If true, excludes disabled user accounts from results</param>
        /// <returns>A list of expired user accounts</returns>
        List<IADUser> FindExpiredUsers(bool ignoreDisabledUsers = true);
        
        /// <summary>
        /// Finds a user by their Distinguished Name (DN)
        /// </summary>
        /// <param name="dn">The Distinguished Name of the user to find</param>
        /// <param name="ignoreDisabledUsers">If true, excludes disabled user accounts from results</param>
        /// <returns>The user with the specified DN, or null if not found</returns>
        IADUser? FindUserByDN(string? dn, bool ignoreDisabledUsers = true);
    }
}