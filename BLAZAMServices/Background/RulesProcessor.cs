using System.Data;
using System.Diagnostics;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Searchers;
using BLAZAM.ActiveDirectory.Services;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Database.Models.Rules;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using BLAZAM.Localization;
using BLAZAM.Logger;
using BLAZAM.Services.Events;
using BLAZAM.Session;
using Microsoft.Extensions.Localization;

namespace BLAZAM.Services.Background
{
    /// <summary>
    /// Background service responsible for processing and executing automation rules
    /// on Active Directory entries, both on schedule and in response to directory events.
    /// </summary>
    [AutoStartBackgroundService(true)]
    public class RulesProcessor : ActiveDirectoryBackgroundServiceBase
    {
        private readonly Dictionary<AutomationRule, Timer> ScheduledRules = new();
        private bool _initialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="RulesProcessor"/> class.
        /// </summary>
        public RulesProcessor(IActiveDirectoryContextFactory activeDirectoryContextFactory, IAppDatabaseFactory dbFactory, IStringLocalizer<AppLocalization> appLocalization) : base(activeDirectoryContextFactory, dbFactory, appLocalization)
        {
            Interval = TimeSpan.FromMinutes(5);
        }

        /// <summary>
        /// Main execution entry point for the background service.
        /// Subscribes to directory entry events and schedules rules for execution.
        /// </summary>
        protected override void Execute(object? state = null)
        {
            if (!_initialized)
            {
                ApplicationEvents.DirectoryEntryEvent.Delegate += ProcessDirectoryEntryChanged;
                _initialized = true;
            }
            ScheduleRules();
        }

        /// <summary>
        /// Schedules enabled automation rules that are due to run soon.
        /// </summary>
        private void ScheduleRules()
        {
            var rules = GetRules();
            var currentTime = DateTime.Now.TimeOfDay;
            var scheduledRules = rules.Where(
                                r => r.Enabled
                            && r.ScheduledRunTime != null
                            && r.DeletedAt == null
                            && r.ScheduledRunTime > currentTime
                            && r.ScheduledRunTime - currentTime < TimeSpan.FromMinutes(11)
                            && r.Trigger == NotificationType.Scheduled
                            ).ToList();
            foreach (var rule in scheduledRules)
            {
                if (!ScheduledRules.ContainsKey(rule))
                {
                    var timeNow = DateTime.Now.TimeOfDay;
                    var timeToRun = rule.ScheduledRunTime;
                    if (timeToRun.HasValue)
                    {
                        // If the scheduled time has already passed today, schedule for tomorrow
                        if (timeToRun.Value < timeNow)
                        {
                            timeToRun = timeToRun.Value.Add(TimeSpan.FromDays(1));
                        }
                        var timeFromRun = timeToRun.Value - timeNow;
                        Timer ruleTimer = new Timer(async (state) => { await ProcessScheduledRule(rule); }, null, (int)timeFromRun.TotalMilliseconds, Timeout.Infinite);
                        ScheduledRules.Add(rule, ruleTimer);
                    }
                }
            }
        }

        /// <summary>
        /// Disposes timers and resources used by the processor.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var scheduledRule in ScheduledRules)
                {
                    scheduledRule.Value.Dispose();
                }
                ScheduledRules.Clear();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Handles directory entry change events and processes applicable rules.
        /// </summary>
        private void ProcessDirectoryEntryChanged(object? sender, DirectoryEntryChangedArgs args)
        {
            if (sender != null && sender.Equals(this))
            {
                return;
            }
            if (args.EventType != ApplicationEventType.Search)
            {
                if (IsApplicationIdentity(args.Entry))
                {
                    return;
                }

                var rules = GetRules();
                if (rules.Count > 0)
                {
                    // Find rules matching the entry type and event trigger
                    var applicableRules = rules
                        .Where(r => r.ActiveDirectoryObjectType.Equals(args.Entry.ObjectType)
                        && r.Trigger.Equals(args.EventType.ToNotificationType()))
                        .OrderBy(r => r.Order).ToList();
                    var ruleProcessingJob = new Job("Process entry change rules");
                    ruleProcessingJob.ThreadPriority = ThreadPriority.Lowest;
                    ruleProcessingJob.StopOnFailedStep = true;
                    foreach (var ruleForEvent in applicableRules)
                    {
                        var ruleStep = new JobStep(ruleForEvent.Name, (step) =>
                        {
                            ProcessMatchedEntry(ruleForEvent, args.Entry);
                            return true;
                        });
                        ruleProcessingJob.AddStep(ruleStep);
                    }
                    _ = ruleProcessingJob.RunAsync();
                }
            }
        }

        /// <summary>
        /// Executes a scheduled automation rule on all matching directory entries.
        /// </summary>
        /// <param name="rule">The automation rule to execute.</param>
        /// <returns>The job representing the rule execution.</returns>
        public async Task<IJob> ProcessScheduledRule(AutomationRule rule)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Loggers.RulesLogger.Information("Executing scheduled rule {@Rule}", rule.Name);

            MarkTriggered(rule);

            Job scheduledRuleJob = new Job(AppLocalization[Lang.Scheduled_Rule], AppLocalization[Lang.Rules] + " " + rule.Name);
            scheduledRuleJob.ThreadPriority = ThreadPriority.Lowest;
            List<IDirectoryEntryAdapter> filteredEntries;

            filteredEntries = GetFilteredEntries(rule);

            // Execute actions for each matched entry
            foreach (var entry in filteredEntries)
            {
                if (IsApplicationIdentity(entry))
                {
                    continue;
                }
                Job entryJob = new Job($"Execute on {entry.CanonicalName}");
                JobStep execApplicableEntriesStep = new($"Execute", (step) =>
                {
                    return ProcessMatchedEntry(rule, entry, entryJob);
                });
                entryJob.AddStep(execApplicableEntriesStep);
                scheduledRuleJob.AddStep(entryJob);
            }
            JobStep logCompletionStep = new("Log completion", (step) =>
            {
                Loggers.RulesLogger.Information("Processing for scheduled rule {@Rule} has finished {@ElapsedTime}", rule.Name, stopwatch.Elapsed);
                return true;
            });
            scheduledRuleJob.AddStep(logCompletionStep);

            _ = scheduledRuleJob.RunAsync();
            return scheduledRuleJob;
        }

        /// <summary>
        /// Determines if the given directory entry represents the application identity,
        /// to prevent rules from acting on the application's own account.
        /// </summary>
        private bool IsApplicationIdentity(IDirectoryEntryAdapter entry)
        {
            using var context = dbFactory.CreateDbContext();
            if (entry is IADUser user &&
                (user.SAMAccountName?.Equals(context.ActiveDirectorySettings.First()?.Username, StringComparison.InvariantCultureIgnoreCase) == true
                || user.UserPrincipalName?.Equals(context.ActiveDirectorySettings.First()?.Username, StringComparison.InvariantCultureIgnoreCase) == true))
            {
                Loggers.RulesLogger.Information("Preventing rule execution on application identity.");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves all directory entries that match the filters defined in the given rule.
        /// </summary>
        /// <param name="rule">The automation rule containing filters.</param>
        /// <returns>List of matching directory entries.</returns>
        public List<IDirectoryEntryAdapter> GetFilteredEntries(AutomationRule rule)
        {
            List<IDirectoryEntryAdapter> matchedEntries = new();
            using (var directory = activeDirectoryContextFactory.CreateActiveDirectoryContext())
            {
                foreach (var orFilter in rule.Filters)
                {
                    // Each OR filter is processed independently; results are accumulated
                    if (!GetFilteredOrEntried(rule, matchedEntries, directory, orFilter))
                    {
                        continue;
                    }
                }
            }
            return matchedEntries;
        }

        /// <summary>
        /// Applies a single OR filter to the directory and adds matching entries to the result list.
        /// </summary>
        private static bool GetFilteredOrEntried(AutomationRule rule, List<IDirectoryEntryAdapter> matchedEntries, IActiveDirectoryContext directory, AutomationRuleOrFilter orFilter)
        {
            ADSearch search = new ADSearch(directory);

            // Handle enabled/disabled filters
            if (orFilter.AndFilters.Any(a => a.Field?.Equals(ActiveDirectoryFields.Enabled) == true && !a.Negate) &&
                !orFilter.AndFilters.Any(a => a.Field?.Equals(ActiveDirectoryFields.Enabled) == true && a.Negate))
            {
                search.EnabledOnly = true;
            }
            // Handle all other fields
            else if (orFilter.AndFilters.Any(a => a.Field?.Equals(ActiveDirectoryFields.Enabled) == true && a.Negate) &&
                !orFilter.AndFilters.Any(a => a.Field?.Equals(ActiveDirectoryFields.Enabled) == true && !a.Negate))
            {
                search.DisabledOnly = true;
            }

            // Handle OU scope filters
            if (orFilter.AndFilters.Any(a => a.Field?.Equals(ActiveDirectoryFields.OU) == true))
            {
                var ouFilters = orFilter.AndFilters.Where(a => a.Field?.Equals(ActiveDirectoryFields.OU) == true)
                    .Select(f => f.Value);
                var ouFilter = ouFilters.OrderBy(f => f?.Length).FirstOrDefault();
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
            }

            // Restrict search to the rule's object type if specified
            if (rule.ActiveDirectoryObjectType != ActiveDirectoryObjectType.All)
            {
                search.ObjectTypeFilter = rule.ActiveDirectoryObjectType;
            }

            // Add all AND filters except for Enabled and OU, which are handled above
            foreach (var andFilters in rule.Filters.Select(x => x.AndFilters))
            {
                foreach (var andFilter in andFilters)
                {
                    if (andFilter.Field?.Equals(ActiveDirectoryFields.Enabled) == false
                        && andFilter.Field?.Equals(ActiveDirectoryFields.OU) == false)
                    {
                        AddAndFilterSearchField(search, andFilter);
                    }
                }
            }
            matchedEntries.AddRange(search.Search());
            return true;
        }

        /// <summary>
        /// Adds a single AND filter as a search field to the ADSearch object.
        /// </summary>
        private static void AddAndFilterSearchField(ADSearch search, AutomationRuleAndFilter? andFilter)
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

        /// <summary>
        /// Processes a single directory entry for a given rule, executing all actions if filters pass.
        /// </summary>
        /// <param name="ruleForEvent">The rule to process.</param>
        /// <param name="entry">The directory entry to process.</param>
        /// <param name="ruleJob">Optional job context for logging/auditing.</param>
        /// <returns>True if processing should continue, false if it should stop.</returns>
        private bool ProcessMatchedEntry(AutomationRule ruleForEvent, IDirectoryEntryAdapter entry, IJob? ruleJob = null)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Task.Delay(50).Wait();

            Loggers.RulesLogger.Information("Rule {@Rule} processing started on {@Entry}.", ruleForEvent.Name, entry.DN);

            Task.Delay(50).Wait();

            if (OrFiltersPass(ruleForEvent, entry))
            {
                try
                {
                    using var context = dbFactory.CreateDbContext();

                    // Update last executed timestamp for the rule
                    var contextRule = context.AutomationRules.First(r => r.Id.Equals(ruleForEvent.Id));
                    contextRule.LastExcecuted = DateTime.UtcNow;
                    context.SaveChanges();
                    Task.Delay(50).Wait();

                }
                catch (Exception ex)
                {
                    Loggers.RulesLogger.Error(ex, "Error while setting LastExcecuted for rule {@Rule}", ruleForEvent.Name, ex);
                }
                foreach (var action in ruleForEvent.Actions)
                {
                    try
                    {
                        Loggers.RulesLogger.Debug("Executing {@Rule} on {@Entry} {@ElapsedTime}", ruleForEvent.Name, entry.CanonicalName);

                        ExecuteAction(ruleForEvent, action, entry, ruleJob);

                        Task.Delay(250).Wait();

                    }
                    catch (Exception ex)
                    {
                        Loggers.RulesLogger.Error(ex, "Error while executing rule action. {@Rule}{@TargetDN}{@Action}", ruleForEvent.Name, entry.DN, action, ex);
                        break;
                    }
                }

                if (ruleForEvent.StopOnThisRule)
                {
                    Loggers.RulesLogger.Information("Processing for rule {@Rule} on {@Entry} has finished {@ElapsedTime}", ruleForEvent.Name, entry.DN, sw.Elapsed);

                    return false;
                }
            }
            Loggers.RulesLogger.Information("Processing for rule {@Rule} on {@Entry} has finished {@ElapsedTime}", ruleForEvent.Name, entry.DN, sw.Elapsed);

            return true;
        }

        /// <summary>
        /// Marks a rule as triggered by updating its LastTriggered timestamp.
        /// </summary>
        private void MarkTriggered(AutomationRule rule)
        {
            using var context = dbFactory.CreateDbContext();
            var contextRule = context.AutomationRules.First(r => r.Id.Equals(rule.Id));
            contextRule.LastTriggered = DateTime.UtcNow;
            context.SaveChanges();
        }

        /// <summary>
        /// Evaluates all OR filters for a rule against a directory entry.
        /// </summary>
        /// <param name="ruleForEvent">The rule to evaluate.</param>
        /// <param name="entry">The directory entry to check.</param>
        /// <returns>True if any OR filter passes, otherwise false.</returns>
        private bool OrFiltersPass(AutomationRule? ruleForEvent, IDirectoryEntryAdapter? entry = null)
        {
            var anyOrTrue = false;
            if (ruleForEvent.Filters.Count > 0)
            {
                foreach (var orFilter in ruleForEvent.Filters)
                {
                    var andTrue = true;
                    foreach (var andFilter in orFilter.AndFilters)
                    {
                        if (!AndFilterTrue(andFilter, entry))
                        {
                            andTrue = false;
                        }
                    }
                    if (andTrue)
                    {
                        anyOrTrue = true;
                        break;
                    }
                }
            }
            else
            {
                anyOrTrue = true;
            }

            return anyOrTrue;
        }

        /// <summary>
        /// Executes a single action on a directory entry as part of a rule.
        /// Handles group assignment, enable/disable, move, field modification, etc.
        /// </summary>
        private void ExecuteAction(AutomationRule rule, AutomationRuleAction action, IDirectoryEntryAdapter entry, IJob? ruleJob = null)
        {
            var eventType = ApplicationEventType.All;
            IDirectoryEntryAdapter? target = null;
            IDirectoryEntryAdapter? origin = null;
            var account = entry as IAccountDirectoryAdapter;
            switch (action.ActionType)
            {
                case AutomationRuleActionType.Assign:
                    eventType = ApplicationEventType.Assign;
                    if (entry is IGroupableDirectoryAdapter groupableEntry)
                    {
                        using (var directory = activeDirectoryContextFactory.CreateActiveDirectoryContext())
                        {
                            var group = directory.Groups.FindGroupBySID(action.GroupSids[0].GroupSid);
                            if (group != null)
                            {
                                target = group;
                                if (groupableEntry.IsAMemberOf(group))
                                {
                                    return;
                                }
                                groupableEntry.AssignTo(group);
                            }
                        }
                    }
                    break;
                case AutomationRuleActionType.Unassign:
                    eventType = ApplicationEventType.Unassign;
                    if (entry is IGroupableDirectoryAdapter groupableEntry2)
                    {
                        using (var directory = activeDirectoryContextFactory.CreateActiveDirectoryContext())
                        {
                            var group = directory.Groups.FindGroupBySID(action.GroupSids[0].GroupSid);
                            if (group != null)
                            {
                                target = group;
                                if (!groupableEntry2.IsAMemberOf(group))
                                {
                                    return;
                                }
                                groupableEntry2.UnassignFrom(group);
                            }
                        }
                    }
                    break;
                case AutomationRuleActionType.Disable:
                    eventType = ApplicationEventType.Modify;
                    if (account != null)
                    {
                        if (!account.Enabled) return;
                        account.Enabled = false;
                    }
                    break;
                case AutomationRuleActionType.Enable:
                    eventType = ApplicationEventType.Modify;
                    if (account != null)
                    {
                        if (account.Enabled) return;
                        account.Enabled = true;
                    }
                    break;
                case AutomationRuleActionType.Unlock:
                    eventType = ApplicationEventType.Modify;
                    if (account != null)
                    {
                        if (!account.LockedOut) return;
                        account.LockedOut = false;
                    }
                    break;
                case AutomationRuleActionType.Lockout:
                    eventType = ApplicationEventType.LockedOut;
                    if (account != null)
                    {
                        if (account.LockedOut) return;
                        account.LockedOut = true;
                    }
                    break;
                case AutomationRuleActionType.Move:
                    eventType = ApplicationEventType.Move;
                    if (action.Data != null)
                    {
                        if (entry.GetParent().DN?.Equals(action.Data, StringComparison.InvariantCultureIgnoreCase) == true)
                        {
                            return;
                        }
                        using var directory = activeDirectoryContextFactory.CreateActiveDirectoryContext();
                        var ou = directory.OUs.FindOuByDN(action.Data);
                        if (ou != null)
                        {
                            target = ou;
                            origin = entry.GetParent();
                            entry.MoveTo(ou);
                        }
                    }
                    break;
                case AutomationRuleActionType.ModifyField:
                    eventType = ApplicationEventType.Modify;
                    if (action.FieldValues.Count > 0)
                    {
                        var field = action.FieldValues[0].CurrentField;
                        var existingValue = entry.GetCustomProperty<object>(field.FieldName);
                        if (existingValue?.ToString()?.Equals(action.FieldValues[0].Value, StringComparison.InvariantCultureIgnoreCase) == true)
                        {
                            return;
                        }
                        entry.SetCustomProperty(field.FieldName, action.FieldValues[0].Value);
                    }
                    break;
            }
            var changes = entry.Changes;
            var result = entry.CommitChanges(ruleJob);
            if (result.FailedSteps.Count == 0)
            {

                ApplicationEvents.DirectoryEntryEvent.Invoke(this, new()
                {
                    Actor = new RulesUserState(dbFactory, rule.Name),
                    Changes = changes,
                    Entry = entry,
                    Target = target,
                    Origin = origin,
                    EventType = eventType
                });
            }
        }

        /// <summary>
        /// Evaluates a single AND filter against a directory entry.
        /// </summary>
        /// <param name="andFilter">The AND filter to evaluate.</param>
        /// <param name="entry">The directory entry to check.</param>
        /// <returns>True if the filter passes, otherwise false.</returns>
        private static bool AndFilterTrue(AutomationRuleAndFilter andFilter, IDirectoryEntryAdapter entry)
        {
            var filterTrue = false;
            try
            {
                if (andFilter.CurrentField is ActiveDirectoryField defaultField)
                {
                    if (andFilter.Field.Equals(ActiveDirectoryFields.OU))
                    {
                        return entry.DN.Contains(andFilter.Value);
                    }
                    switch (andFilter.Operator)
                    {
                        case ActiveDirectoryFieldOperator.EqualTo:
                            filterTrue = entry.PropertyValueEquals(defaultField.PropertyName, andFilter.Value);
                            break;
                        case ActiveDirectoryFieldOperator.HistoricalTimeFrame:
                            var dateValue3 = entry.GetPropertyValue(defaultField.PropertyName);
                            if (dateValue3 is DateTime dateTime3)
                            {
                                filterTrue = dateTime3 > DateTime.Now - andFilter.TimeFrame;
                            }
                            else if (dateValue3 is long fileTime)
                            {
                                filterTrue = fileTime < DateTime.Now.ToFileTimeUtc();
                            }
                            break;
                        case ActiveDirectoryFieldOperator.FutureTimeFrame:
                            var dateValue4 = entry.GetPropertyValue(defaultField.PropertyName);
                            if (dateValue4 is DateTime dateTime4)
                            {
                                filterTrue = dateTime4 > DateTime.Now - andFilter.TimeFrame;
                            }
                            else if (dateValue4 is long fileTime)
                            {
                                filterTrue = fileTime < DateTime.Now.ToFileTimeUtc();
                            }
                            break;
                        case ActiveDirectoryFieldOperator.StartsWith:
                            filterTrue = entry.GetPropertyValue(defaultField.PropertyName).ToString().StartsWith(andFilter.Value.ToString(), StringComparison.InvariantCultureIgnoreCase);
                            break;
                        case ActiveDirectoryFieldOperator.EndsWith:
                            filterTrue = entry.GetPropertyValue(defaultField.PropertyName).ToString().EndsWith(andFilter.Value.ToString(), StringComparison.InvariantCultureIgnoreCase);
                            break;
                        case ActiveDirectoryFieldOperator.AfterNow:
                            var dateValue = entry.GetPropertyValue(defaultField.PropertyName);
                            if (dateValue is DateTime dateTime)
                            {
                                filterTrue = dateTime > DateTime.Now;
                            }
                            else if (dateValue is long fileTime)
                            {
                                filterTrue = fileTime < DateTime.Now.ToFileTimeUtc();
                            }
                            break;
                        case ActiveDirectoryFieldOperator.BeforeNow:
                            var dateValue2 = entry.GetPropertyValue(defaultField.PropertyName);
                            if (dateValue2 is DateTime dateTime2)
                            {
                                filterTrue = dateTime2 < DateTime.Now;
                            }
                            else if (dateValue2 is long fileTime)
                            {
                                filterTrue = fileTime < DateTime.Now.ToFileTimeUtc();
                            }
                            break;
                        case ActiveDirectoryFieldOperator.Contains:
                            var propertyValue = entry.GetPropertyValue(defaultField.PropertyName).ToString();
                            filterTrue = propertyValue?.Contains(andFilter.Value.ToString(), StringComparison.InvariantCultureIgnoreCase) == true;
                            break;
                        case ActiveDirectoryFieldOperator.Boolean:
                            if (entry.GetPropertyValue(defaultField.PropertyName) is bool boolValue)
                            {
                                filterTrue = boolValue == true;
                            }
                            break;
                    }
                }
                else if (andFilter.CurrentField is CustomActiveDirectoryField customField)
                {
                    switch (andFilter.Operator)
                    {
                        case ActiveDirectoryFieldOperator.EqualTo:
                            filterTrue = entry.GetCustomProperty<object>(customField.FieldName).Equals(andFilter.Value);
                            break;
                        case ActiveDirectoryFieldOperator.HistoricalTimeFrame:
                            var raw = entry.GetCustomProperty<object>(customField.FieldName);
                            var dateValue3 = raw.AdsValueToDateTime();
                            if (dateValue3 is DateTime dateTime3)
                            {
                                filterTrue = dateTime3 > DateTime.Now - andFilter.TimeFrame;
                            }
                            break;
                        case ActiveDirectoryFieldOperator.FutureTimeFrame:
                            var raw2 = entry.GetCustomProperty<object>(customField.FieldName);
                            var dateValue4 = raw2.AdsValueToDateTime();
                            if (dateValue4 is DateTime dateTime4)
                            {
                                filterTrue = dateTime4 > DateTime.Now - andFilter.TimeFrame;
                            }
                            break;
                        case ActiveDirectoryFieldOperator.StartsWith:
                            filterTrue = entry.GetPropertyValue(customField.FieldName).ToString().StartsWith(andFilter.Value.ToString());
                            break;
                        case ActiveDirectoryFieldOperator.EndsWith:
                            filterTrue = entry.GetPropertyValue(customField.FieldName).ToString().EndsWith(andFilter.Value.ToString());
                            break;
                        case ActiveDirectoryFieldOperator.AfterNow:
                            var raw3 = entry.GetCustomProperty<object>(customField.FieldName);
                            var dateValue = raw3.AdsValueToDateTime(); if (dateValue is DateTime dateTime)
                            {
                                filterTrue = dateTime > DateTime.Now;
                            }
                            break;
                        case ActiveDirectoryFieldOperator.BeforeNow:
                            var raw4 = entry.GetCustomProperty<object>(customField.FieldName);
                            var dateValue2 = raw4.AdsValueToDateTime();
                            if (dateValue2 is DateTime dateTime2)
                            {
                                filterTrue = dateTime2 < DateTime.Now;
                            }
                            break;
                        case ActiveDirectoryFieldOperator.Contains:
                            var propertyValue = entry.GetPropertyValue(customField.FieldName).ToString();
                            filterTrue = propertyValue?.Contains(andFilter.Value.ToString(), StringComparison.InvariantCultureIgnoreCase) == true;
                            break;
                        case ActiveDirectoryFieldOperator.Boolean:
                            if (entry.GetPropertyValue(customField.FieldName) is bool boolValue)
                            {
                                filterTrue = boolValue == true;
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Loggers.RulesLogger.Error(ex, "Error checking and filter {@Filter}", andFilter);
            }
            if (andFilter.Negate)
            {
                filterTrue = !filterTrue;
            }
            return filterTrue;
        }

        /// <summary>
        /// Retrieves all enabled, non-deleted, and non-expired automation rules from the database.
        /// </summary>
        private List<AutomationRule> GetRules()
        {
            try
            {
                using var context = dbFactory.CreateDbContext();
                return context.AutomationRules.Where(
                        r => r.DeletedAt == null
                        && r.Enabled
                        && (r.ExpirationDate == null || r.ExpirationDate > DateTime.Now)
                    ).ToList();
            }
            catch (Exception ex)
            {
                Loggers.RulesLogger.Debug(ex, "Error loading rules");
                return [];
            }
        }
    }
}
