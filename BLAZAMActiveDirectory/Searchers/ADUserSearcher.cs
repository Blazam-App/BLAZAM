using BLAZAM.ActiveDirectory;
using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using System.Security.Cryptography;

namespace BLAZAM.ActiveDirectory.Searchers
{
    public class ADUserSearcher : ADSearcher, IADUserSearcher
    {

        public ADUserSearcher(IActiveDirectoryContext directory) : base(directory)
        {
        }

        public async Task<List<IADUser>> FindUsersByStringAsync(string? searchTerm, bool? ignoreDisabledUsers = true, bool exactMatch = false)
        {
            return await Task.Run(() =>
            {
                return FindUsersByString(searchTerm, ignoreDisabledUsers, exactMatch);
            });
        }

        public List<IADUser> FindUsersByString(string? searchTerm, bool? ignoreDisabledUsers = true, bool exactMatch = false)
        {
            return new ADSearch(Directory)
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.User,
                EnabledOnly = ignoreDisabledUsers,
                GeneralSearchTerm = searchTerm,
                ExactMatch = exactMatch

            }.Search<ADUser, IADUser>();
        }
        public IADUser? FindUserByUsername(string? searchTerm, bool? ignoreDisabledUsers = true)
        {
            return new ADSearch(Directory)
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.User,
                EnabledOnly = ignoreDisabledUsers,
                Fields = new()
                {
                    SamAccountName = searchTerm
                },
                ExactMatch = true

            }.Search<ADUser, IADUser>().FirstOrDefault();

        }

        public async Task<List<IADUser>> FindLockedOutUsersAsync(bool? ignoreDisabledUsers = true)
        {
            return await Task.Run(() =>
            {
                return FindLockedOutUsers(ignoreDisabledUsers);
            });
        }

        public List<IADUser> FindLockedOutUsers(bool? ignoreDisabledUsers = true)
        {
            return new ADSearch(Directory)
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.User,
                EnabledOnly = ignoreDisabledUsers,
                Fields = new()
                {
                    LockoutTime = 1
                }

            }.Search<ADUser, IADUser>();


        }


        public async Task<List<IADUser>> FindNewUsersAsync(int maxAgeInDays = 14, bool? ignoreDisabledUsers = true)
        {
            return await Task.Run(() =>
            {
                return FindNewUsers(maxAgeInDays, ignoreDisabledUsers);
            });
        }

        public List<IADUser> FindNewUsers(int maxAgeInDays = 14, bool? ignoreDisabledUsers = true)
        {

            var threeMonthsAgo = DateTime.Today - TimeSpan.FromDays(maxAgeInDays);
            var results = new ADSearch(Directory)
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.User,
                EnabledOnly = ignoreDisabledUsers,
                Fields = new()
                {
                    Created = threeMonthsAgo
                }

            }.Search<ADUser, IADUser>();
            return results.OrderByDescending(u => u.Created).ToList();

        }

        public async Task<List<IADUser>> FindChangedPasswordUsersAsync(bool? ignoreDisabledUsers = true)
        {
            return await Task.Run(() =>
            {
                return FindChangedPasswordUsers(ignoreDisabledUsers);
            });
        }

        public List<IADUser> FindChangedPasswordUsers(bool? ignoreDisabledUsers = true)
        {
            var threeMonthsAgo = DateTime.Today - TimeSpan.FromDays(90);

            var tstamp = threeMonthsAgo.ToFileTimeUtc();

            var results = new ADSearch(Directory)
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.User,
                EnabledOnly = ignoreDisabledUsers,
                Fields = new()
                {
                    PasswordLastSet = threeMonthsAgo.ToFileTimeUtc().ToString()
                }

            }.Search<ADUser, IADUser>();
            return results.OrderByDescending(u => u.PasswordLastSet).ToList();

            //var tstamp = threeMonthsAgo.ToString("yyyyMMddHHmmss.fZ");
            string UserSearchFieldsQuery = "(pwdLastSet>=" + tstamp + ")";

            return SearchObjects(UserSearchFieldsQuery, ActiveDirectoryObjectType.User, 1000, ignoreDisabledUsers).Cast<IADUser>().OrderByDescending(u => u.PasswordLastSet).ToList();

        }

        public async Task<List<IADUser>> FindChangedUsersAsync(bool? ignoreDisabledUsers = true)
        {
            return await Task.Run(() =>
            {
                return FindChangedUsers(ignoreDisabledUsers);
            });
        }

        public List<IADUser> FindChangedUsers(bool? ignoreDisabledUsers = true, int daysBackToSearch = 90)
        {
            var threeMonthsAgo = DateTime.Today - TimeSpan.FromDays(daysBackToSearch);

            var tstamp = threeMonthsAgo.ToString("yyyyMMddHHmmss.fZ");
            string UserSearchFieldsQuery = "(whenChanged>=" + tstamp + ")";

            return SearchObjects(UserSearchFieldsQuery, ActiveDirectoryObjectType.User, 1000, ignoreDisabledUsers).Cast<IADUser>().OrderByDescending(u => u.LastChanged).ToList();

        }
        public IADUser? FindUserBySID(string? sid)
        {
            if (sid == null) return null;
            return new ADSearch(Directory)
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.User,
                EnabledOnly = false,
                Fields = new() { SID = sid },
                ExactMatch = true

            }.Search<ADUser, IADUser>().FirstOrDefault();
        }

        public IADUser? FindUsersByContainerName(string? searchTerm, bool? ignoreDisabledUsers = true, bool exactMatch = false)
        {

            return new ADSearch(Directory)
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.User,
                EnabledOnly = ignoreDisabledUsers,
                Fields = new() { CN = searchTerm },
                ExactMatch = exactMatch

            }.Search<ADUser, IADUser>().FirstOrDefault();

        }
    }
}
