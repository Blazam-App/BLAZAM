using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.ActiveDirectory.Models;
using System.Security.Cryptography;

namespace BLAZAM.Common.Data.ActiveDirectory.Searchers
{
    public class ADUserSearcher : ADSearcher, IADUserSearcher
    {

        public ADUserSearcher(IActiveDirectory directory) : base(directory)
        {
        }

        public async Task<List<IADUser>> FindUsersByStringAsync(string searchTerm, bool? ignoreDisabledUsers = true, bool exactMatch = false)
        {
            return await Task.Run(() =>
            {
                return FindUsersByString(searchTerm, ignoreDisabledUsers, exactMatch);
            });
        }

        public List<IADUser> FindUsersByString(string searchTerm, bool? ignoreDisabledUsers = true, bool exactMatch = false)
        {
            return new ADSearch()
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.User,
                EnabledOnly = ignoreDisabledUsers,
                GeneralSearchTerm = searchTerm,
                ExactMatch = exactMatch

            }.Search<ADUser, IADUser>();
        }
        public IADUser? FindUserByUsername(string searchTerm, bool? ignoreDisabledUsers = true)
        {
            return new ADSearch()
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

        public List<IADUser>? FindLockedOutUsers(bool? ignoreDisabledUsers = true)
        {
            return new ADSearch()
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.User,
                EnabledOnly = ignoreDisabledUsers,
                Fields = new()
                {
                    LockoutTime = 1
                }

            }.Search<ADUser, IADUser>();

            string UserSearchFieldsQuery = "(lockoutTime>=1)";

            return new List<IADUser>(ConvertTo<ADUser>(SearchObjects(UserSearchFieldsQuery, ActiveDirectoryObjectType.User, 50, ignoreDisabledUsers)));

        }


        public async Task<List<IADUser>> FindNewUsersAsync(bool? ignoreDisabledUsers = true)
        {
            return await Task.Run(() =>
            {
                return FindNewUsers(ignoreDisabledUsers);
            });
        }

        public List<IADUser>? FindNewUsers(bool? ignoreDisabledUsers = true)
        {

            var threeMonthsAgo = DateTime.Today - TimeSpan.FromDays(90);

            var tstamp = threeMonthsAgo.ToString("yyyyMMddHHmmss.fZ");
            var results = new ADSearch()
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.User,
                EnabledOnly = ignoreDisabledUsers,
                Fields = new()
                {
                    Created = tstamp }

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

        public List<IADUser>? FindChangedPasswordUsers(bool? ignoreDisabledUsers = true)
        {
            var threeMonthsAgo = DateTime.Today - TimeSpan.FromDays(90);

            var tstamp = threeMonthsAgo.ToString("yyyyMMddHHmmss.fZ");
            string UserSearchFieldsQuery = "(pwdLastSet>=" + tstamp + ")";

            return new List<IADUser>(ConvertTo<ADUser>(SearchObjects(UserSearchFieldsQuery, ActiveDirectoryObjectType.User, 1000, ignoreDisabledUsers)).OrderByDescending(u => u.PasswordLastSet));

        }

        public async Task<List<IADUser>> FindChangedUsersAsync(bool? ignoreDisabledUsers = true)
        {
            return await Task.Run(() =>
            {
                return FindChangedUsers(ignoreDisabledUsers);
            });
        }

        public List<IADUser>? FindChangedUsers(bool? ignoreDisabledUsers = true, int daysBackToSearch = 90)
        {
            var threeMonthsAgo = DateTime.Today - TimeSpan.FromDays(daysBackToSearch);

            var tstamp = threeMonthsAgo.ToString("yyyyMMddHHmmss.fZ");
            string UserSearchFieldsQuery = "(whenChanged>=" + tstamp + ")";

            return new List<IADUser>(ConvertTo<ADUser>(SearchObjects(UserSearchFieldsQuery, ActiveDirectoryObjectType.User, 1000, ignoreDisabledUsers)).OrderByDescending(u => u.LastChanged));

        }
        public IADUser? FindUserBySID(string? sid)
        {
            if (sid == null) return null;
            return new ADSearch()
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.User,
                EnabledOnly = false,
                Fields = new() { SID = sid },
                ExactMatch = true

            }.Search<ADUser, IADUser>().FirstOrDefault();
        }

        public IADUser? FindUsersByContainerName(string? searchTerm, bool? ignoreDisabledUsers = true, bool exactMatch = false)
        {

            return new ADSearch()
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.User,
                EnabledOnly = ignoreDisabledUsers,
                Fields = new() { CN = searchTerm },
                ExactMatch = exactMatch

            }.Search<ADUser, IADUser>().FirstOrDefault();

        }
    }
}
