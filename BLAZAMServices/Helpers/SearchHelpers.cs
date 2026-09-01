using BLAZAM.ActiveDirectory;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Searchers;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Rules;
using BLAZAM.Logger;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Services.Helpers
{
    public static class SearchHelpers
    {

        /// <summary>
        /// Retrieves all directory entries that match the filters defined in the given rule.
        /// </summary>
        /// <param name="rule">The automation rule containing filters.</param>
        /// <returns>List of matching directory entries.</returns>
        public static async Task<List<IDirectoryEntryAdapter>> GetFilteredEntries(this List<AutomationRuleOrFilter> filters,ActiveDirectoryObjectType searchObjectType, IAppDatabaseFactory dbFactory, IActiveDirectoryContext directory)
        {
            List<IDirectoryEntryAdapter> matchedEntries = [];

           
                foreach (var orFilter in filters)
                {
                    // Each OR filter is processed independently; results are accumulated
                    if (!GetFilteredOrEntries(orFilter, searchObjectType, matchedEntries, directory))
                    {
                        continue;
                    }
                }
            
            List<IDirectoryEntryAdapter> matchedEntries2 = new();

            foreach (var entry in matchedEntries)
            {
                if (!(await ShouldSkipEntry(entry,dbFactory,directory)))
                {
                    matchedEntries2.Add(entry);
                }
            }
            return matchedEntries2;
        }



        /// <summary>
        /// Applies a single OR filter to the directory and adds matching entries to the result list.
        /// </summary>
        public static bool GetFilteredOrEntries(AutomationRuleOrFilter filter,ActiveDirectoryObjectType searchObjectType, List<IDirectoryEntryAdapter> matchedEntries, IActiveDirectoryContext directory)
        {
            ADSearch search = new ADSearch(directory);

            HandleEnabledDisabledFilter(filter, search);

            if (!HandleOUScopeFilter(filter, directory, search))
            {
                return false;
            }

            if (searchObjectType != ActiveDirectoryObjectType.All)
            {
                search.ObjectTypeFilter = searchObjectType;
            }
            search.AddAndFilters(filter);
            var rawResults = search.Search();
            matchedEntries.AddRange(rawResults.Where(e=>e.CanRead));
            return true;
        }

        private static void AddAndFilters(this ADSearch search,AutomationRuleOrFilter filter)
        {
            foreach (var andFilter in filter.AndFilters)
            {
                if (andFilter.Field?.Equals(ActiveDirectoryFields.Enabled) == false
                    && andFilter.Field?.Equals(ActiveDirectoryFields.OU) == false)
                {
                    AddAndFilterSearchField(search, andFilter);
                }
            }
        }

       

        /// <summary>
        /// Adds a single AND filter as a search field to the ADSearch object.
        /// </summary>
        private static void AddAndFilterSearchField(this ADSearch search, AutomationRuleAndFilter? andFilter)
        {
            if (andFilter == null)
            {
                throw new ArgumentNullException(nameof(andFilter));
            }
            try
            {
                var fieldValue = new ADFieldValue()
                {
                    Field = andFilter.CurrentField,
                    Value = andFilter.TimeFrame == null ? andFilter.Value : andFilter.TimeFrame,
                    Operator = andFilter.Operator,
                    Negate = andFilter.Negate
                };
                if (andFilter.CurrentField is ActiveDirectoryField defaultField || andFilter.CurrentField is CustomActiveDirectoryField)
                {
                    search.FieldValues.Add(fieldValue);
                }
            }
            catch (Exception ex)
            {
                Loggers.RulesLogger.Warning(ex, "Unable to set search field value {@Field}{@Value}", andFilter.Field, andFilter.Value);
            }
        }

        private static void HandleEnabledDisabledFilter(AutomationRuleOrFilter orFilter, ADSearch search)
        {
            var enabledFilters = orFilter.AndFilters.Where(a => a.Field?.Equals(ActiveDirectoryFields.Enabled) == true);
            var hasEnable = enabledFilters.Any(f => !f.Negate);
            var hasDisable = enabledFilters.Any(f => f.Negate);

            if (hasEnable && !hasDisable)
            {
                search.EnabledOnly = true;
            }
            else if (hasDisable && !hasEnable)
            {
                search.DisabledOnly = true;
            }
        }

        private static bool HandleOUScopeFilter(AutomationRuleOrFilter orFilter, IActiveDirectoryContext directory, ADSearch search)
        {
            var ouFilter = orFilter.AndFilters
                .Where(a => a.Field?.Equals(ActiveDirectoryFields.OU) == true)
                .Select(f => f.Value)
                .OrderBy(f => f?.Length)
                .FirstOrDefault();

            if (ouFilter != null && !ouFilter.IsNullOrEmpty())
            {
                var ou = directory.OUs.FindOuByDN(ouFilter);
                ou?.EnsureDirectoryEntry();
                var ouDE = ou?.DirectoryEntry;
                if (ouDE != null)
                {
                    search.SearchRoot = ouDE;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        public static async Task<bool> ShouldSkipEntry(this IDirectoryEntryAdapter entry,  IAppDatabaseFactory dbFactory, IActiveDirectoryContext directory)
        {
            using var context = await dbFactory.CreateDbContextAsync();
            if (IsApplicationIdentity(entry,dbFactory))
            {
                return true;
            }
            if (entry is IGroupableDirectoryAdapter groupableEntry)
            {
                var ruleSetttings = await context.GlobalAutomationRuleSettings.FirstOrDefaultAsync();
                if (ruleSetttings == null)
                {
                    await SeedExcludedGroups(dbFactory,directory);
                    ruleSetttings = await context.GlobalAutomationRuleSettings.FirstOrDefaultAsync();
                }
                foreach (var groupGuid in ruleSetttings.ExcludedGroups)
                {
                    var group = directory.FindGlobalEntryByGuid(groupGuid.Guid);
                    if (groupableEntry.IsANestedMemberOf(group as IADGroup))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        public static async Task SeedExcludedGroups(this IAppDatabaseFactory dbFactory, IActiveDirectoryContext directory)
        {
            using var context = await dbFactory.CreateDbContextAsync();
            var existing = await context.GlobalAutomationRuleSettings.FirstOrDefaultAsync();
            if (existing == null)
            {
                existing = new();


                // Try to resolve the actual SIDs for the current domain/forest
                

                if (directory.Status != DirectoryConnectionStatus.OK)
                {
                    Loggers.ActiveDirectoryLogger.Information("Cancelling excluded rule group seeding because connection is not established.");
                    return;
                }


                var domainAdminsGroup = directory.FindGlobalEntryBySid(directory.DomainSid + "-512") as IADGroup;
                var domainControllersGroup = directory.FindGlobalEntryBySid(directory.DomainSid + "-516") as IADGroup;
                var enterpriseDCGroup = directory.FindGlobalEntryBySid("S-1-5-9") as IADGroup;
                var enterpriseAdminsGroup = directory.FindGlobalEntryBySid(directory.DomainSid + "-519") as IADGroup;
                bool seededAny = false;
                if (domainAdminsGroup != null)
                {
                    existing.ExcludedGroups.Add(new() { Guid = domainAdminsGroup.Guid.Value });
                    seededAny = true;
                }
                if (domainControllersGroup != null)
                {
                    existing.ExcludedGroups.Add(new() { Guid = domainControllersGroup.Guid.Value });
                    seededAny = true;

                }
                if (enterpriseAdminsGroup != null)
                {
                    existing.ExcludedGroups.Add(new() { Guid = enterpriseAdminsGroup.Guid.Value });
                    seededAny = true;

                }
                if (enterpriseDCGroup != null)
                {
                    existing.ExcludedGroups.Add(new() { Guid = enterpriseDCGroup.Guid.Value });
                    seededAny = true;

                }
                if (seededAny)
                {
                    context.GlobalAutomationRuleSettings.Add(existing);
                    await context.SaveChangesAsync();
                }

            }
        }

        /// <summary>
        /// Determines if the given directory entry represents the application identity,
        /// to prevent rules from acting on the application's own account.
        /// </summary>
        private static bool IsApplicationIdentity(IDirectoryEntryAdapter entry,IAppDatabaseFactory dbFactory)
        {
            using var context = dbFactory.CreateDbContext();
            var adUsername = context.ActiveDirectorySettings.FirstOrDefault()?.Username;
            if (entry is IADUser user &&
                (user.SAMAccountName?.Equals(adUsername, StringComparison.InvariantCultureIgnoreCase) == true
                || user.UserPrincipalName?.Equals(adUsername, StringComparison.InvariantCultureIgnoreCase) == true
                || adUsername?.EndsWith("\\" + user.SAMAccountName, StringComparison.InvariantCultureIgnoreCase) == true))
            {
                Loggers.RulesLogger.Information("Preventing rule execution on application identity.");
                return true;
            }
            return false;
        }



    }
}
