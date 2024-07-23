using BLAZAM.ActiveDirectory;
using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Helpers;
using BLAZAM.Common.Data;
using BLAZAM.Logger;

namespace BLAZAM.ActiveDirectory.Searchers
{
    
    public class ADGroupSearcher : ADSearcher, IADGroupSearcher
    {
        /// <summary>
        /// This might need to go, or at least be set to remove cached entries after some period of time
        /// </summary>
        private static Dictionary<string, IADGroup> GroupSIDCache = new Dictionary<string, IADGroup>();


        public ADGroupSearcher(IActiveDirectoryContext directory) : base(directory)
        {
        }

        public async Task<List<IADGroup>> FindGroupByStringAsync(string searchTerm, bool exactMatch = false)
        {
            return await Task.Run(() =>
            {

                return FindGroupByString(searchTerm, exactMatch);
            });
        }
        /// <summary>
        /// Find all matching groups by Distinguished Name fragment.
        /// This is not always an exact match search. For exact match, be sure
        /// to use the entire group's Distinguished Name including the CN=. This
        /// is a limitation of LDAP.
        /// </summary>
        /// <param name="dn">The Distinguished Name fragment to find in groups</param>
        /// <returns>All groups with the distinguished name fragment in their own distinguished name</returns>
        public List<IADGroup> FindGroupByDN(string dn)
        {
            return new ADSearch()
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.Group,
                EnabledOnly = false,
                Fields = new()
                {
                    DN = dn
                },
                ExactMatch = true

            }.Search<ADGroup, IADGroup>();

        }

        public List<IADGroup> FindGroupByString(string searchTerm, bool exactMatch = false)
        {
            return new ADSearch()
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.Group,
                GeneralSearchTerm = searchTerm,
                ExactMatch = exactMatch

            }.Search<ADGroup, IADGroup>();



        }

        public async Task<List<IADGroup>> FindNewGroupsAsync(int maxAgeInDays = 14)
        {
            return await Task.Run(() =>
            {
                return FindNewGroups(maxAgeInDays);
            });
        }

        public List<IADGroup>? FindNewGroups(int maxAgeInDays = 14)
        {

            var threeMonthsAgo = DateTime.Today - TimeSpan.FromDays(maxAgeInDays);
            var results = new ADSearch()
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.Group,
                Fields = new()
                {
                    Created = threeMonthsAgo
                }

            }.Search<ADGroup, IADGroup>();
            return results.OrderByDescending(u => u.Created).ToList();

        }


        public List<IGroupableDirectoryAdapter>? GetAllNestedMembers(IADGroup group)
        {
            string UserSearchFieldsQuery = "(&(memberOf:1.2.840.113556.1.4.1941:=" + group.DN + "))";
            return SearchObjects(UserSearchFieldsQuery, ActiveDirectoryObjectType.User).Cast<IGroupableDirectoryAdapter>().ToList();

        }


        public IADGroup? FindGroupBySID(byte[] groupSID) => FindGroupBySID(groupSID.ToSidString());

        public IADGroup? FindGroupBySID(string groupSID)
        {
            return new ADSearch()
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.Group,
                Fields = new()
                {
                    SID = groupSID
                },
                ExactMatch = true

            }.Search<ADGroup, IADGroup>().FirstOrDefault();

        }

        /// <summary>
        /// Find all matching groups by Distinguished Name fragments.
        /// This is not always an exact match search. For exact match, be sure
        /// to use the entire group's Distinguished Name including the CN=
        /// </summary>
        /// <param name="list">A list of Distinguished Name fragments</param>
        /// <returns>All groups with any of the distinguished name fragments in their own distinguished name</returns>
        public List<IADGroup> FindGroupsByDN(List<string>? list)
        {

            List<IADGroup> foundGroups = new List<IADGroup>();
            if (list != null)
            {

                foreach (string groupDN in list)
                {
                    var group = new ADSearch()
                    {
                        ObjectTypeFilter = ActiveDirectoryObjectType.Group,
                        Fields = new()
                        {
                            DN = groupDN
                        }

                    }.Search<ADGroup, IADGroup>().FirstOrDefault();

                   // query = "(distinguishedName=" + groupDN + ")";
                   // var group = SearchObjects(query, ActiveDirectoryObjectType.Group, 1);
                    if (group != null)
                    {
                       // var adGroup = ConvertTo<ADGroup>(group);
                        foundGroups.Add(group);
                    }
                    else
                    {
                        Loggers.ActiveDirectryLogger.Warning("Unable to find group in list by DN", list, groupDN, group);
                    }
                }

            }
            return foundGroups;


        }
        public List<IADUser> GetDirectUserMembers(IADGroup group, bool ignoreDisabledUsers = true)
        {
            /*
            return new ADSearch()
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.User,
                MemberOf = group.DN,
                EnabledOnly=ignoreDisabledUsers

            }.Search<ADUser, IADUser>();
            */
            string UserSearchFieldsQuery = "(memberOf=" + group.DN + ")";
            return SearchObjects(UserSearchFieldsQuery, ActiveDirectoryObjectType.User, 500, ignoreDisabledUsers).Cast<IADUser>().ToList();


        }
        public List<IADGroup> GetGroupMembers(IADGroup group)
        {
            /*
            return new ADSearch()
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.Group,
                MemberOf = group.DN,

            }.Search<ADGroup, IADGroup>();
            */
            string UserSearchFieldsQuery = "(memberOf=" + group.DN + ")";
            return SearchObjects(UserSearchFieldsQuery, ActiveDirectoryObjectType.Group, 500).Cast<IADGroup>().ToList();


        }

        public bool IsAMemberOf(IADGroup group, IGroupableDirectoryAdapter? userOrGroup, bool v, bool ignoreDisabledUsers = true)
        {
            return new ADSearch()
            {
                Fields = new()
                {
                    DN = userOrGroup.DN,
                    NestedMemberOf = group
                },
                ExactMatch = false

            }.Search<ADUser, IADUser>().Count>0;
            string UserSearchFieldsQuery = "(&(memberOf:1.2.840.113556.1.4.1941:=" + group.DN + ")(distinguishedName=" + userOrGroup.DN + "))";
            return SearchObjects(UserSearchFieldsQuery, userOrGroup.ObjectType, 50, ignoreDisabledUsers)?.Count > 0;

        }
    }
}
