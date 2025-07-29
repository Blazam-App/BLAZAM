using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Data;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Services; // For WmiFactory
using BLAZAM.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace BLAZAM.ActiveDirectory.Mocks
{
    // Helper predicate methods (can be static within a helper class or private in each searcher)
    internal static class MockSearchPredicates
    {
        public static bool IsMatch(string? propertyValue, string? searchTerm, bool exactMatch)
        {
            if (string.IsNullOrEmpty(searchTerm)) return true; // No search term means it's a match for this property
            if (string.IsNullOrEmpty(propertyValue)) return false;

            return exactMatch
                ? propertyValue.Equals(searchTerm, StringComparison.OrdinalIgnoreCase)
                : propertyValue.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsRecentlyCreated(DateTime? createdDate, int maxAgeInDays)
        {
            if (!createdDate.HasValue) return false;
            return (DateTime.UtcNow - createdDate.Value).TotalDays <= maxAgeInDays;
        }

        public static bool IsRecentlyChanged(DateTime? changedDate, int daysBackToSearch)
        {
            if (!changedDate.HasValue) return false;
            return (DateTime.UtcNow - changedDate.Value).TotalDays <= daysBackToSearch;
        }

        public static bool IsDisabled(IAccountDirectoryAdapter account) => account?.Disabled ?? true; // Treat null as disabled for safety in filters

        public static bool IsNotDisabled(IAccountDirectoryAdapter account) => !IsDisabled(account);

    }

    public class MockADUserSearcher : IADUserSearcher
    {
        private readonly MockActiveDirectoryContext _mockContext;

        public MockADUserSearcher(MockActiveDirectoryContext context)
        {
            _mockContext = context;
        }

        private IEnumerable<IADUser> GetBaseQuery(bool ignoreDisabledUsers)
        {
            var query = _mockContext.GetAllAdapters().OfType<IADUser>();
            if (ignoreDisabledUsers)
            {
                query = query.Where(u => MockSearchPredicates.IsNotDisabled(u));
            }
            return query;
        }

        public List<IADUser> FindUsersByString(string? searchTerm, bool ignoreDisabledUsers = true, bool exactMatch = false)
        {
            return GetBaseQuery(ignoreDisabledUsers)
                .Where(u => MockSearchPredicates.IsMatch(u.DisplayName, searchTerm, exactMatch) ||
                             MockSearchPredicates.IsMatch(u.SAMAccountName, searchTerm, exactMatch) ||
                             MockSearchPredicates.IsMatch(u.GivenName, searchTerm, exactMatch) ||
                             MockSearchPredicates.IsMatch(u.Sn, searchTerm, exactMatch) ||
                             MockSearchPredicates.IsMatch(u.Email, searchTerm, exactMatch))
                .ToList();
        }
        public Task<List<IADUser>> FindUsersByStringAsync(string? searchTerm, bool ignoreDisabledUsers = true, bool exactMatch = false)
            => Task.FromResult(FindUsersByString(searchTerm, ignoreDisabledUsers, exactMatch));

        public IADUser? FindUsersByContainerName(string? searchTerm, bool ignoreDisabledUsers = true, bool exactMatch = true)
        {
            // "ContainerName" typically refers to the CN (Common Name) part of the DN.
            // For users, DisplayName or SAMAccountName might be used if CN isn't directly exposed or consistently just "CN=..."
            return GetBaseQuery(ignoreDisabledUsers)
                .FirstOrDefault(u => MockSearchPredicates.IsMatch(u.CanonicalName, searchTerm, exactMatch));
        }


        public List<IADUser>? FindLockedOutUsers(bool ignoreDisabledUsers = true)
        {
            return GetBaseQuery(ignoreDisabledUsers)
                .Where(u => u.LockedOut)
                .ToList();
        }
        public Task<List<IADUser>> FindLockedOutUsersAsync(bool ignoreDisabledUsers = true)
            => Task.FromResult(FindLockedOutUsers(ignoreDisabledUsers) ?? new List<IADUser>());

        public IADUser? FindUserBySID(string? sid)
        {
            if (string.IsNullOrEmpty(sid)) return null;
            try
            {
                var sidToCompare = new SecurityIdentifier(sid);
                return _mockContext.GetAllAdapters().OfType<IADUser>()
                    .FirstOrDefault(u => u.SID != null && new SecurityIdentifier(u.SID, 0).Equals(sidToCompare));
            }
            catch { return null; }
        }

        public IADUser? FindUserByUsername(string? searchTerm, bool ignoreDisabledUsers = true, bool exactMatch = false)
        {
            return GetBaseQuery(ignoreDisabledUsers)
                .FirstOrDefault(u => MockSearchPredicates.IsMatch(u.SAMAccountName, searchTerm, exactMatch));
        }
        public IADUser? FindUserByDN(string? searchTerm, bool ignoreDisabledUsers = true)
        {
            return GetBaseQuery(ignoreDisabledUsers)
                .FirstOrDefault(u => MockSearchPredicates.IsMatch(u.SAMAccountName, searchTerm,true));
        }

        public List<IADUser>? FindNewUsers(int maxAgeInDays = 14, bool ignoreDisabledUsers = true)
        {
            return GetBaseQuery(ignoreDisabledUsers)
                .Where(u => MockSearchPredicates.IsRecentlyCreated(u.Created, maxAgeInDays))
                .ToList();
        }

        public Task<List<IADUser>> FindNewUsersAsync(int maxAgeInDays = 14, bool ignoreDisabledUsers = true)
            => Task.FromResult(FindNewUsers(maxAgeInDays, ignoreDisabledUsers) ?? new List<IADUser>());

        public List<IADUser>? FindChangedUsers(bool ignoreDisabledUsers = true, int daysBackToSearch = 90)
        {
            return GetBaseQuery(ignoreDisabledUsers)
                .Where(u => MockSearchPredicates.IsRecentlyChanged(u.LastChanged, daysBackToSearch))
                .ToList();
        }
        public Task<List<IADUser>> FindChangedUsersAsync(bool ignoreDisabledUsers = true)
            => Task.FromResult(FindChangedUsers(ignoreDisabledUsers) ?? new List<IADUser>());

        public List<IADUser>? FindChangedPasswordUsers(bool ignoreDisabledUsers = true) // Assuming default daysBack for password change
        {
            int daysBackToSearch = 90; // Default as per FindChangedUsers, adjust if different logic needed
            return GetBaseQuery(ignoreDisabledUsers)
                .Where(u => u.PasswordLastSet.HasValue && MockSearchPredicates.IsRecentlyChanged(u.PasswordLastSet, daysBackToSearch))
                .ToList();
        }
        public Task<List<IADUser>> FindChangedPasswordUsersAsync(bool ignoreDisabledUsers = true)
            => Task.FromResult(FindChangedPasswordUsers(ignoreDisabledUsers) ?? new List<IADUser>());


        public List<IADUser> FindExpiredUsers(bool ignoreDisabledUsers = true)
        {
            return GetBaseQuery(ignoreDisabledUsers)
                .Where(u => u.ExpireTime.HasValue && u.ExpireTime.Value < DateTime.UtcNow)
                .ToList();
        }
    }

    public class MockADGroupSearcher : IADGroupSearcher
    {
        private readonly MockActiveDirectoryContext _mockContext;
        public MockADGroupSearcher(MockActiveDirectoryContext context) { _mockContext = context; }

        private IEnumerable<IADGroup> GetBaseQuery() => _mockContext.GetAllAdapters().OfType<IADGroup>();

        public IADGroup? FindGroupBySID(byte[] groupSID)
        {
            if (groupSID == null) return null;
            try
            {
                var sidToCompare = new SecurityIdentifier(groupSID, 0);
                return GetBaseQuery()
                   .FirstOrDefault(g => g.SID != null && new SecurityIdentifier(g.SID, 0).Equals(sidToCompare));
            }
            catch { return null; }
        }
        public IADGroup? FindGroupBySID(string groupSID)
        {
            if (string.IsNullOrEmpty(groupSID)) return null;
            try
            {
                return FindGroupBySID(groupSID.ToSidByteArray());
            }
            catch { return null; }
        }


        public List<IADGroup> FindGroupByString(string searchTerm, bool exactMatch = false)
        {
            return GetBaseQuery()
                .Where(g => MockSearchPredicates.IsMatch(g.DisplayName, searchTerm, exactMatch) ||
                             MockSearchPredicates.IsMatch(g.SAMAccountName, searchTerm, exactMatch))
                .ToList();
        }
        public Task<List<IADGroup>> FindGroupByStringAsync(string searchTerm, bool exactMatch = false)
            => Task.FromResult(FindGroupByString(searchTerm, exactMatch));

        public List<IADGroup> FindGroupsByDN(List<string>? list)
        {
            if (list == null || !list.Any()) return new List<IADGroup>();
            var dnSet = new HashSet<string>(list, StringComparer.OrdinalIgnoreCase);
            return GetBaseQuery().Where(g => g.DN != null && dnSet.Contains(g.DN)).ToList();
        }

        public List<IADGroup> GetGroupMembers(IADGroup group) // Group members that are groups
        {
            if (group?.DirectoryEntry is MockDirectoryEntry mockEntry)
            {
                var memberDNs = mockEntry.GetPropertyValues("member")?.OfType<string>().ToList() ?? new List<string>();
                return _mockContext.GetAllAdapters()
                                   .OfType<IADGroup>()
                                   .Where(adapter => adapter.DN != null && memberDNs.Contains(adapter.DN, StringComparer.OrdinalIgnoreCase))
                                   .ToList();
            }
            return new List<IADGroup>();
        }


        public List<IADUser> GetDirectUserMembers(IADGroup group, bool ignoreDisabledUsers = true)
        {
            if (group?.DirectoryEntry is MockDirectoryEntry mockEntry)
            {
                var memberDNs = mockEntry.GetPropertyValues("member")?.OfType<string>().ToList() ?? new List<string>();
                var usersQuery = _mockContext.GetAllAdapters().OfType<IADUser>();
                if (ignoreDisabledUsers)
                {
                    usersQuery = usersQuery.Where(u => MockSearchPredicates.IsNotDisabled(u));
                }
                return usersQuery.Where(u => u.DN != null && memberDNs.Contains(u.DN, StringComparer.OrdinalIgnoreCase)).ToList();
            }
            return new List<IADUser>();
        }

        public bool IsAMemberOf(IADGroup? group, IGroupableDirectoryAdapter? userOrGroup, bool v, bool ignoreDisabledUsers = true)
        {
            // 'v' parameter is not clearly defined in usage, assuming it's for future use or implies something about recursion not handled here.
            // This mock checks direct membership.
            if (group == null || userOrGroup == null || userOrGroup.DN == null || group.DN == null) return false;

            if (userOrGroup.DirectoryEntry is MockDirectoryEntry userMockEntry)
            {
                var memberOfDNs = userMockEntry.GetPropertyValues("memberOf")?.OfType<string>().ToList() ?? new List<string>();
                return memberOfDNs.Contains(group.DN, StringComparer.OrdinalIgnoreCase);
            }
            // Fallback if memberOf isn't directly on the user, check the group's members
            if (group.DirectoryEntry is MockDirectoryEntry groupMockEntry)
            {
                var memberDNs = groupMockEntry.GetPropertyValues("member")?.OfType<string>().ToList() ?? new List<string>();
                return memberDNs.Contains(userOrGroup.DN, StringComparer.OrdinalIgnoreCase);
            }
            return false;
        }

        public List<IADGroup>? FindNewGroups(int maxAgeInDays = 14)
        {
            return GetBaseQuery()
                .Where(g => MockSearchPredicates.IsRecentlyCreated(g.Created, maxAgeInDays))
                .ToList();
        }
        public Task<List<IADGroup>> FindNewGroupsAsync(int maxAgeInDays = 14)
            => Task.FromResult(FindNewGroups(maxAgeInDays) ?? new List<IADGroup>());

        public List<IGroupableDirectoryAdapter>? GetAllNestedMembers(IADGroup group)
        {
            if (group == null) return new List<IGroupableDirectoryAdapter>();

            var allMembers = new List<IGroupableDirectoryAdapter>();
            var processedGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var queue = new Queue<IADGroup>();

            queue.Enqueue(group);
            processedGroups.Add(group.DN);

            while (queue.Count > 0)
            {
                var currentGroup = queue.Dequeue();

                // Get direct user members
                if (currentGroup.DirectoryEntry is MockDirectoryEntry currentGroupMockEntry)
                {
                    var memberDNs = currentGroupMockEntry.GetPropertyValues("member")?.OfType<string>().ToList() ?? new List<string>();
                    foreach (var memberDN in memberDNs)
                    {
                        var memberAdapter = _mockContext.GetDirectoryEntryByDN(memberDN) as IGroupableDirectoryAdapter;
                        if (memberAdapter != null)
                        {
                            if (!allMembers.Any(m => m.DN == memberAdapter.DN)) // Avoid duplicates
                            {
                                allMembers.Add(memberAdapter);
                            }

                            if (memberAdapter is IADGroup subgroup && !processedGroups.Contains(subgroup.DN))
                            {
                                queue.Enqueue(subgroup);
                                processedGroups.Add(subgroup.DN);
                            }
                        }
                    }
                }
            }
            return allMembers;
        }
    }

    public class MockADOUSearcher : IADOUSearcher
    {
        private readonly MockActiveDirectoryContext _mockContext;
        public MockADOUSearcher(MockActiveDirectoryContext context) { _mockContext = context; }

        private IEnumerable<IADOrganizationalUnit> GetBaseQuery() => _mockContext.GetAllAdapters().OfType<IADOrganizationalUnit>();

        private bool IsChildOf(string? childDN, string? parentDN)
        {
            if (string.IsNullOrEmpty(childDN) || string.IsNullOrEmpty(parentDN)) return false;
            // Naive check: childDN must end with ",<parentDN>" and be longer.
            // More robust: parse DNs, but for mock this might be sufficient.
            return childDN.EndsWith("," + parentDN, StringComparison.OrdinalIgnoreCase) && childDN.Length > parentDN.Length;
        }
        private bool IsDirectChildOf(string? childDN, string? parentDN)
        {
            if (string.IsNullOrEmpty(childDN) || string.IsNullOrEmpty(parentDN)) return false;
            var childParts = childDN.Split(',');
            var parentParts = parentDN.Split(',');
            if (childParts.Length != parentParts.Length + 1) return false;
            return childDN.EndsWith("," + parentDN, StringComparison.OrdinalIgnoreCase);
        }


        public List<IADUser> FindSubUsersByDN(string searchBaseDN)
        {
            return _mockContext.GetAllAdapters().OfType<IADUser>()
                .Where(u => u.DN != null && IsChildOf(u.DN, searchBaseDN)).ToList();
        }
        public List<IADComputer> FindSubComputerByDN(string searchBaseDN)
        {
            return _mockContext.GetAllAdapters().OfType<IADComputer>()
                .Where(c => c.DN != null && IsChildOf(c.DN, searchBaseDN)).ToList();
        }
        public List<IADGroup> FindSubGroupsByDN(string searchBaseDN)
        {
            return _mockContext.GetAllAdapters().OfType<IADGroup>()
                .Where(g => g.DN != null && IsChildOf(g.DN, searchBaseDN)).ToList();
        }
        public List<IADOrganizationalUnit> FindSubOusByDN(string searchBaseDN) // Direct children
        {
            return GetBaseQuery().Where(ou => ou.DN != null && IsDirectChildOf(ou.DN, searchBaseDN)).ToList();
        }

        public IADOrganizationalUnit? FindOuByDN(string searchTerm)
        {
            return GetBaseQuery().FirstOrDefault(ou => MockSearchPredicates.IsMatch(ou.DN, searchTerm, true));
        }

        public IADOrganizationalUnit GetApplicationRootOU()
        {
            var appRootDN = _mockContext.ConnectionSettings?.ApplicationBaseDN;
            if (string.IsNullOrEmpty(appRootDN))
                throw new InvalidOperationException("ApplicationBaseDN is not configured in mock settings.");
            return FindOuByDN(appRootDN) ?? throw new KeyNotFoundException($"Application root OU '{appRootDN}' not found in mock data.");
        }

        public List<IADOrganizationalUnit> FindOuByString(string searchTerm)
        {
            return GetBaseQuery()
                .Where(ou => MockSearchPredicates.IsMatch(ou.Name, searchTerm, false) || // 'Name' for OU is typically its 'ou' attribute
                             MockSearchPredicates.IsMatch(ou.CanonicalName, searchTerm, false)) // DisplayName might be same or different
                .ToList();
        }
        public Task<List<IADOrganizationalUnit>> FindOuByStringAsync(string searchTerm)
            => Task.FromResult(FindOuByString(searchTerm));


        public List<IADOrganizationalUnit> FindNewOUs(int maxAgeInDays = 14)
        {
            return GetBaseQuery()
                .Where(ou => MockSearchPredicates.IsRecentlyCreated(ou.Created, maxAgeInDays))
                .ToList();
        }
        public Task<List<IADOrganizationalUnit>> FindNewOUsAsync(int maxAgeInDays = 14)
            => Task.FromResult(FindNewOUs(maxAgeInDays));
    }

    public class MockADComputerSearcher : IADComputerSearcher
    {
        private readonly MockActiveDirectoryContext _mockContext;
        public WmiFactory WmiFactory { get; } // Can be null or a simple mock for basic tests

        public MockADComputerSearcher(MockActiveDirectoryContext context, WmiFactory wmiFactory = null)
        {
            _mockContext = context;
            WmiFactory = wmiFactory; // Assign if provided, otherwise it's null
        }

        private IEnumerable<IADComputer> GetBaseQuery(bool ignoreDisabled)
        {
            var query = _mockContext.GetAllAdapters().OfType<IADComputer>();
            if (ignoreDisabled)
            {
                query = query.Where(c => MockSearchPredicates.IsNotDisabled(c));
            }
            return query;
        }

        public List<IADComputer> FindByString(string searchTerm, bool ignoreDisabled) // Interface has ignoreDisabled, not exactMatch
        {
            return GetBaseQuery(ignoreDisabled)
                .Where(c => MockSearchPredicates.IsMatch(c.DisplayName, searchTerm, false) ||
                             MockSearchPredicates.IsMatch(c.SAMAccountName, searchTerm, false) ||
                             MockSearchPredicates.IsMatch(c.OperatingSystem, searchTerm, false))
                .ToList();
        }
        public Task<List<IADComputer>> FindByStringAsync(string searchTerm, bool ignoreDisabled = true)
            => Task.FromResult(FindByString(searchTerm, ignoreDisabled));

        public List<IADComputer> FindNewComputers(int maxAgeInDays = 14, bool ignoreDisabledComputers = false)
        {
            return GetBaseQuery(ignoreDisabledComputers)
                .Where(c => MockSearchPredicates.IsRecentlyCreated(c.Created, maxAgeInDays))
                .ToList();
        }
        public Task<List<IADComputer>> FindNewComputersAsync(int maxAgeInDays = 14, bool ignoreDisabledComputers = false)
            => Task.FromResult(FindNewComputers(maxAgeInDays, ignoreDisabledComputers));
    }

    public class MockADContactSearcher : IADContactSearcher
    {
        private readonly MockActiveDirectoryContext _mockContext;
        public MockADContactSearcher(MockActiveDirectoryContext context) { _mockContext = context; }

        private IEnumerable<IADContact> GetBaseQuery() => _mockContext.GetAllAdapters().OfType<IADContact>();
        // ignoreDisabledUsers for contacts is a bit unusual. If contacts can be linked to user accounts and thus "disabled",
        // that logic would need IAccountDirectoryAdapter properties. Assuming contacts are generally not "disabled" in AD sense.

        public List<IADContact> FindChangedContacts(bool ignoreDisabledUsers = true, int daysBackToSearch = 90)
        {
            return GetBaseQuery()
                .Where(c => MockSearchPredicates.IsRecentlyChanged(c.LastChanged, daysBackToSearch))
                .ToList();
        }
        public Task<List<IADContact>> FindChangedContactsAsync(bool ignoreDisabledUsers = true) // daysBackToSearch defaults in interface impl
            => Task.FromResult(FindChangedContacts(ignoreDisabledUsers));


        public IADContact? FindContactsByContainerName(string? searchTerm, bool exactMatch = false)
        {
            // Assuming "ContainerName" refers to CN or DisplayName for contacts
            return GetBaseQuery()
                .FirstOrDefault(c => MockSearchPredicates.IsMatch(c.DisplayName, searchTerm, exactMatch));
        }

        public IADContact? FindContactsByGUID(byte[]? guid)
        {
            if (guid == null) return null;
            return GetBaseQuery()
                .FirstOrDefault(c => c.Guid != null && c.Guid.SequenceEqual(guid));
        }


        public List<IADContact> FindContactsByString(string? searchTerm, bool exactMatch = false)
        {
            return GetBaseQuery()
                .Where(c => MockSearchPredicates.IsMatch(c.DisplayName, searchTerm, exactMatch) ||
                             MockSearchPredicates.IsMatch(c.Email, searchTerm, exactMatch) ||
                             MockSearchPredicates.IsMatch(c.GivenName, searchTerm, exactMatch) ||
                             MockSearchPredicates.IsMatch(c.Sn, searchTerm, exactMatch))
                .ToList();
        }
        public Task<List<IADContact>> FindContactsByStringAsync(string? searchTerm, bool exactMatch = false)
            => Task.FromResult(FindContactsByString(searchTerm, exactMatch));


        public List<IADContact> FindExpiredContacts()
        {
            // Contacts don't typically expire. This might look for a custom 'msExchHideFromAddressLists' or similar if that's the intent.
            // For now, returning empty as standard AD contacts don't have an expiry like user accounts.
            return new List<IADContact>();
        }

        public List<IADContact> FindNewContacts(int maxAgeInDays = 14, bool ignoreDisabledUsers = true)
        {
            return GetBaseQuery()
                .Where(c => c.ObjectType==ActiveDirectoryObjectType.Contact && MockSearchPredicates.IsRecentlyCreated(c.Created, maxAgeInDays))
                .ToList();
        }
        public Task<List<IADContact>> FindNewContactsAsync(int maxAgeInDays = 14, bool ignoreDisabledUsers = true)
            => Task.FromResult(FindNewContacts(maxAgeInDays, ignoreDisabledUsers));
    }

    public class MockADPrinterSearcher : IADPrinterSearcher
    {
        private readonly MockActiveDirectoryContext _mockContext;
        public MockADPrinterSearcher(MockActiveDirectoryContext context) { _mockContext = context; }

        // ignoreDisabledPrinters is not a standard AD concept for printer objects. They are either present or not.
        // Will assume the bool? is for future-proofing or a custom interpretation. For now, it won't filter by a "disabled" state.
        private IEnumerable<IADPrinter> GetBaseQuery(bool? ignoreDisabledPrinters = true) => _mockContext.GetAllAdapters().OfType<IADPrinter>();


        public List<IADPrinter> FindChangedPrinters(bool? ignoreDisabledPrinters = true, int daysBackToSearch = 90)
        {
            return GetBaseQuery(ignoreDisabledPrinters)
                .Where(p => MockSearchPredicates.IsRecentlyChanged(p.LastChanged, daysBackToSearch))
                .ToList();
        }
        public Task<List<IADPrinter>> FindChangedPrintersAsync(bool? ignoreDisabledPrinters = true)
            => Task.FromResult(FindChangedPrinters(ignoreDisabledPrinters));

        public List<IADPrinter> FindNewPrinters(int maxAgeInDays = 14, bool? ignoreDisabledPrinters = true)
        {
            return GetBaseQuery(ignoreDisabledPrinters)
                .Where(p => MockSearchPredicates.IsRecentlyCreated(p.Created, maxAgeInDays))
                .ToList();
        }
        public Task<List<IADPrinter>> FindNewPrintersAsync(int maxAgeInDays = 14, bool? ignoreDisabledPrinters = true)
            => Task.FromResult(FindNewPrinters(maxAgeInDays, ignoreDisabledPrinters));


        public IADPrinter? FindPrinterByName(string? searchTerm, bool? ignoreDisabledPrinters = true)
        {
            // PrinterName is an attribute, also check CN (DisplayName for adapter)
            return GetBaseQuery(ignoreDisabledPrinters)
                .FirstOrDefault(p => MockSearchPredicates.IsMatch(p.PrinterName, searchTerm, true) ||
                                      MockSearchPredicates.IsMatch((p as IGroupableDirectoryAdapter)?.DisplayName, searchTerm, true));
        }

        public IADPrinter? FindPrintersByContainerName(string? searchTerm, bool? ignoreDisabledPrinters = true, bool exactMatch = false)
        {
            // "ContainerName" likely means CN (DisplayName for adapter)
            return GetBaseQuery(ignoreDisabledPrinters)
                 .FirstOrDefault(p => MockSearchPredicates.IsMatch(p.CanonicalName, searchTerm, exactMatch));
        }

        public List<IADPrinter> FindPrintersByString(string? searchTerm, bool? ignoreDisabledPrinters = true, bool exactMatch = false)
        {
            return GetBaseQuery(ignoreDisabledPrinters)
                .Where(p => MockSearchPredicates.IsMatch(p.PrinterName, searchTerm, exactMatch) ||
                             MockSearchPredicates.IsMatch(p.CanonicalName, searchTerm, exactMatch) ||
                             MockSearchPredicates.IsMatch(p.Location, searchTerm, exactMatch) ||
                             MockSearchPredicates.IsMatch(p.UncName, searchTerm, exactMatch))
                .ToList();
        }
        public Task<List<IADPrinter>> FindPrintersByStringAsync(string? searchTerm, bool? ignoreDisabledPrinters = true, bool exactMatch = false)
            => Task.FromResult(FindPrintersByString(searchTerm, ignoreDisabledPrinters, exactMatch));
    }

    public class MockADBitLockerSearcher : IADBitLockerSearcher
    {
        private readonly MockActiveDirectoryContext _mockContext;
        public MockADBitLockerSearcher(MockActiveDirectoryContext context) { _mockContext = context; }

        private IEnumerable<IADBitLockerRecovery> GetBaseQuery() => _mockContext.GetAllAdapters().OfType<IADBitLockerRecovery>();

        public List<IADBitLockerRecovery> FindByRecoveryId(string searchTerm) // searchTerm is the Recovery GUID as a string
        {
            if (!Guid.TryParse(searchTerm, out var searchGuid)) return new List<IADBitLockerRecovery>();

            return GetBaseQuery()
                .Where(bl => bl.RecoveryId.HasValue && bl.RecoveryId.Value == searchGuid)
                .ToList();
        }
        public Task<List<IADBitLockerRecovery>> FindByRecoveryIdAsync(string searchTerm)
            => Task.FromResult(FindByRecoveryId(searchTerm));


        public List<IADBitLockerRecovery> FindByComputer(IADComputer computer)
        {
            if (computer == null || computer.DN == null) return new List<IADBitLockerRecovery>();
            // BitLocker recovery objects are typically children of the computer object they pertain to.
            // So, their DN will be something like "CN=<RecoveryGUID>,<ComputerDN>"
            return GetBaseQuery()
                .Where(bl => bl.DN != null && bl.DN.EndsWith("," + computer.DN, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        public Task<List<IADBitLockerRecovery>> FindByComputerAsync(IADComputer computer)
            => Task.FromResult(FindByComputer(computer));
    }
}