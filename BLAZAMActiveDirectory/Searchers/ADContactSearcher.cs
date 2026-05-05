using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;

namespace BLAZAM.ActiveDirectory.Searchers
{
    public class ADContactSearcher(IActiveDirectoryContext directory) : ADSearcher(directory), IADContactSearcher
    {
        public async Task<List<IADContact>> FindContactsByStringAsync(string? searchTerm, bool exactMatch = false)
        {
            return await Task.Run(() =>
            {
                return FindContactsByString(searchTerm, exactMatch);
            });
        }

        public List<IADContact> FindContactsByString(string? searchTerm, bool exactMatch = false)
        {
            return new ADSearch(Directory)
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.Contact,
                GeneralSearchTerm = searchTerm,
                ExactMatch = exactMatch

            }.Search<ADContact, IADContact>();
        }



        public List<IADContact> FindExpiredContacts()
        {
            return new ADSearch(Directory)
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.Contact,
                Fields = new()
                {
                    ExpireTime = DateTime.UtcNow
                }

            }.Search<ADContact, IADContact>();


        }


        public async Task<List<IADContact>> FindNewContactsAsync(int maxAgeInDays = 14, bool ignoreDisabledUsers = true)
        {
            return await Task.Run(() =>
            {
                return FindNewContacts(maxAgeInDays, ignoreDisabledUsers);
            });
        }

        public List<IADContact> FindNewContacts(int maxAgeInDays = 14, bool ignoreDisabledUsers = true)
        {

            var threeMonthsAgo = DateTime.Today - TimeSpan.FromDays(maxAgeInDays);
            var results = new ADSearch(Directory)
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.Contact,
                EnabledOnly = ignoreDisabledUsers,
                Fields = new()
                {
                    Created = threeMonthsAgo
                }

            }.Search<ADContact, IADContact>();
            return results.OrderByDescending(u => u.Created).ToList();

        }



        public async Task<List<IADContact>> FindChangedContactsAsync(bool ignoreDisabledUsers = true)
        {
            return await Task.Run(() =>
            {
                return FindChangedContacts(ignoreDisabledUsers);
            });
        }

        public List<IADContact> FindChangedContacts(bool ignoreDisabledUsers = true, int daysBackToSearch = 90)
        {
            var threeMonthsAgo = DateTime.Today - TimeSpan.FromDays(daysBackToSearch);

            var tstamp = threeMonthsAgo.ToString("yyyyMMddHHmmss.fZ");
            string UserSearchFieldsQuery = "(whenChanged>=" + tstamp + ")";

            return SearchObjects(UserSearchFieldsQuery, ActiveDirectoryObjectType.Contact, 1000, ignoreDisabledUsers)?.Cast<IADContact>().OrderByDescending(u => u.LastChanged).ToList();

        }

        public IADContact? FindContactsByGUID(byte[]? guid)
        {
            return Directory.FindGlobalEntryByGuid(guid) as IADContact;
        }
        public IADContact? FindContactsByGUID(Guid? guid)
        {
            return Directory.FindGlobalEntryByGuid(guid?.ToByteArray()) as IADContact;
        }

        public IADContact? FindContactsByContainerName(string? searchTerm, bool exactMatch = false)
        {

            return new ADSearch(Directory)
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.Contact,
                Fields = new() { CN = searchTerm },
                ExactMatch = exactMatch

            }.Search<ADContact, IADContact>().FirstOrDefault();

        }
    }
}
