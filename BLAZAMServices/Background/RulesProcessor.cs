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
                    if (!GetFilteredOrEntries(rule, matchedEntries, directory, orFilter))
                    {
                        continue;
                    }
                }
            }
            return matchedEntries;
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

        private static void AddAndFilters(AutomationRule rule, ADSearch search)
        {
            foreach (var andFilter in rule.Filters.SelectMany(x => x.AndFilters))
            {
                if (andFilter.Field?.Equals(ActiveDirectoryFields.Enabled) == false
                    && andFilter.Field?.Equals(ActiveDirectoryFields.OU) == false)
                {
                    AddAndFilterSearchField(search, andFilter);
                }
            }
        }
        /// <summary>
        /// Applies a single OR filter to the directory and adds matching entries to the result list.
        /// </summary>
        private static bool GetFilteredOrEntries(AutomationRule rule, List<IDirectoryEntryAdapter> matchedEntries, IActiveDirectoryContext directory, AutomationRuleOrFilter orFilter)
        {
            ADSearch search = new ADSearch(directory);

            HandleEnabledDisabledFilter(orFilter, search);

            if (!HandleOUScopeFilter(orFilter, directory, search)) return false;

            if (rule.ActiveDirectoryObjectType != ActiveDirectoryObjectType.All)
            {
                search.ObjectTypeFilter = rule.ActiveDirectoryObjectType;
            }

            AddAndFilters(rule, search);

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
        private void ExecuteAssignAction(AutomationRuleAction action, IDirectoryEntryAdapter entry, out IDirectoryEntryAdapter? target, out ApplicationEventType eventType)
        {
            target = null;
            eventType = ApplicationEventType.Assign;
            if (entry is IGroupableDirectoryAdapter groupableEntry)
            {
                using var directory = activeDirectoryContextFactory.CreateActiveDirectoryContext();
                var group = directory.Groups.FindGroupBySID(action.GroupSids[0].GroupSid);
                if (group != null)
                {
                    target = group;
                    if (groupableEntry.IsAMemberOf(group))
                    {
                        eventType = ApplicationEventType.All;
                        return;
                    };
                    groupableEntry.AssignTo(group);
                }
            }
        }

        private void ExecuteUnassignAction(AutomationRuleAction action, IDirectoryEntryAdapter entry, out IDirectoryEntryAdapter? target, out ApplicationEventType eventType)
        {
            target = null;
            eventType = ApplicationEventType.Unassign;
            if (entry is IGroupableDirectoryAdapter groupableEntry)
            {
                using var directory = activeDirectoryContextFactory.CreateActiveDirectoryContext();
                var group = directory.Groups.FindGroupBySID(action.GroupSids[0].GroupSid);
                if (group != null)
                {
                    target = group;
                    if (!groupableEntry.IsAMemberOf(group))
                    {
                        eventType = ApplicationEventType.All;
                        return;
                    }
                    groupableEntry.UnassignFrom(group);
                }
            }
        }

        private void ExecuteDisableAction(IDirectoryEntryAdapter entry, out ApplicationEventType eventType)
        {
            eventType = ApplicationEventType.Modify;
            if (entry is IAccountDirectoryAdapter account)
            {
                if (!account.Enabled)
                {
                    eventType = ApplicationEventType.All;
                    return;
                }
                account.Enabled = false;
            }
        }

        private void ExecuteEnableAction(IDirectoryEntryAdapter entry, out ApplicationEventType eventType)
        {
            eventType = ApplicationEventType.Modify;
            if (entry is IAccountDirectoryAdapter account)
            {
                if (account.Enabled)
                {
                    eventType = ApplicationEventType.All;
                    return;
                }
                account.Enabled = true;
            }
        }

        private void ExecuteUnlockAction(IDirectoryEntryAdapter entry, out ApplicationEventType eventType)
        {
            eventType = ApplicationEventType.Modify;
            if (entry is IAccountDirectoryAdapter account)
            {
                if (!account.LockedOut)
                {
                    eventType = ApplicationEventType.All;
                    return;
                }
                account.LockedOut = false;
            }
        }

        private void ExecuteLockoutAction(IDirectoryEntryAdapter entry, out ApplicationEventType eventType)
        {
            eventType = ApplicationEventType.LockedOut;
            if (entry is IAccountDirectoryAdapter account)
            {
                if (account.LockedOut)
                {
                    eventType = ApplicationEventType.All;
                    return;
                }
                account.LockedOut = true;
            }
        }

        private void ExecuteMoveAction(AutomationRuleAction action, IDirectoryEntryAdapter entry, out IDirectoryEntryAdapter? target, out IDirectoryEntryAdapter? origin, out ApplicationEventType eventType)
        {
            target = null;
            origin = null;
            eventType = ApplicationEventType.Move;
            if (action.Data != null)
            {
                if (entry.GetParent().DN?.Equals(action.Data, StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    eventType = ApplicationEventType.All;
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
        }

        private void ExecuteModifyFieldAction(AutomationRuleAction action, IDirectoryEntryAdapter entry, out ApplicationEventType eventType)
        {
            eventType = ApplicationEventType.Modify;
            if (action.FieldValues.Count > 0)
            {
                var field = action.FieldValues[0].CurrentField;
                var existingValue = entry.GetCustomProperty<object>(field.FieldName);
                if (existingValue?.ToString()?.Equals(action.FieldValues[0].Value, StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    eventType = ApplicationEventType.All;
                    return;
                }
                entry.SetCustomProperty(field.FieldName, action.FieldValues[0].Value);
            }
        }
        private void ExecuteAction(AutomationRule rule, AutomationRuleAction action, IDirectoryEntryAdapter entry, IJob? ruleJob = null)
        {
            var eventType = ApplicationEventType.All;
            IDirectoryEntryAdapter? target = null;
            IDirectoryEntryAdapter? origin = null;


            switch (action.ActionType)
            {
                case AutomationRuleActionType.Assign:
                    ExecuteAssignAction(action, entry, out target, out eventType);
                    break;
                case AutomationRuleActionType.Unassign:
                    ExecuteUnassignAction(action, entry, out target, out eventType);
                    break;
                case AutomationRuleActionType.Disable:
                    ExecuteDisableAction(entry, out eventType);
                    break;
                case AutomationRuleActionType.Enable:
                    ExecuteEnableAction(entry, out eventType);
                    break;
                case AutomationRuleActionType.Unlock:
                    ExecuteUnlockAction(entry, out eventType);
                    break;
                case AutomationRuleActionType.Lockout:
                    ExecuteLockoutAction(entry, out eventType);
                    break;
                case AutomationRuleActionType.Move:
                    ExecuteMoveAction(action, entry, out target, out origin, out eventType);
                    break;
                case AutomationRuleActionType.ModifyField:
                    ExecuteModifyFieldAction(action, entry, out eventType);
                    break;
            }

            if (eventType == ApplicationEventType.All) return;

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
        private static bool CheckDefaultFieldFilter(AutomationRuleAndFilter andFilter, IDirectoryEntryAdapter entry, ActiveDirectoryField defaultField)
        {
            if (andFilter.Field.Equals(ActiveDirectoryFields.OU))
            {
                return entry.DN.Contains(andFilter.Value);
            }

            switch (andFilter.Operator)
            {
                case ActiveDirectoryFieldOperator.EqualTo:
                    return entry.PropertyValueEquals(defaultField.PropertyName, andFilter.Value);
                case ActiveDirectoryFieldOperator.HistoricalTimeFrame:
                    var dateValue3 = entry.GetPropertyValue(defaultField.PropertyName);
                    if (dateValue3 is DateTime dateTime3)
                    {
                        return dateTime3 > DateTime.Now - andFilter.TimeFrame;
                    }
                    else if (dateValue3 is long fileTime)
                    {
                        return fileTime < DateTime.Now.ToFileTimeUtc();
                    }
                    break;
                case ActiveDirectoryFieldOperator.FutureTimeFrame:
                    var dateValue4 = entry.GetPropertyValue(defaultField.PropertyName);
                    if (dateValue4 is DateTime dateTime4)
                    {
                        return dateTime4 > DateTime.Now - andFilter.TimeFrame;
                    }
                    else if (dateValue4 is long fileTime)
                    {
                        return fileTime < DateTime.Now.ToFileTimeUtc();
                    }
                    break;
                case ActiveDirectoryFieldOperator.StartsWith:
                    return entry.GetPropertyValue(defaultField.PropertyName).ToString().StartsWith(andFilter.Value.ToString(), StringComparison.InvariantCultureIgnoreCase);
                case ActiveDirectoryFieldOperator.EndsWith:
                    return entry.GetPropertyValue(defaultField.PropertyName).ToString().EndsWith(andFilter.Value.ToString(), StringComparison.InvariantCultureIgnoreCase);
                case ActiveDirectoryFieldOperator.AfterNow:
                    var dateValue = entry.GetPropertyValue(defaultField.PropertyName);
                    if (dateValue is DateTime dateTime)
                    {
                        return dateTime > DateTime.Now;
                    }
                    else if (dateValue is long fileTime)
                    {
                        return fileTime < DateTime.Now.ToFileTimeUtc();
                    }
                    break;
                case ActiveDirectoryFieldOperator.BeforeNow:
                    var dateValue2 = entry.GetPropertyValue(defaultField.PropertyName);
                    if (dateValue2 is DateTime dateTime2)
                    {
                        return dateTime2 < DateTime.Now;
                    }
                    else if (dateValue2 is long fileTime)
                    {
                        return fileTime < DateTime.Now.ToFileTimeUtc();
                    }
                    break;
                case ActiveDirectoryFieldOperator.Contains:
                    var propertyValue = entry.GetPropertyValue(defaultField.PropertyName).ToString();
                    return propertyValue?.Contains(andFilter.Value.ToString(), StringComparison.InvariantCultureIgnoreCase) == true;
                case ActiveDirectoryFieldOperator.Boolean:
                    if (entry.GetPropertyValue(defaultField.PropertyName) is bool boolValue)
                    {
                        return boolValue == true;
                    }
                    break;
            }
            return false;
        }

        private static bool CheckCustomFieldFilter(AutomationRuleAndFilter andFilter, IDirectoryEntryAdapter entry, CustomActiveDirectoryField customField)
        {
            switch (andFilter.Operator)
            {
                case ActiveDirectoryFieldOperator.EqualTo:
                    return entry.GetCustomProperty<object>(customField.FieldName).Equals(andFilter.Value);
                case ActiveDirectoryFieldOperator.HistoricalTimeFrame:
                    var raw = entry.GetCustomProperty<object>(customField.FieldName);
                    var dateValue3 = raw.AdsValueToDateTime();
                    if (dateValue3 is DateTime dateTime3)
                    {
                        return dateTime3 > DateTime.Now - andFilter.TimeFrame;
                    }
                    break;
                case ActiveDirectoryFieldOperator.FutureTimeFrame:
                    var raw2 = entry.GetCustomProperty<object>(customField.FieldName);
                    var dateValue4 = raw2.AdsValueToDateTime();
                    if (dateValue4 is DateTime dateTime4)
                    {
                        return dateTime4 > DateTime.Now - andFilter.TimeFrame;
                    }
                    break;
                case ActiveDirectoryFieldOperator.StartsWith:
                    return entry.GetPropertyValue(customField.FieldName).ToString().StartsWith(andFilter.Value.ToString());
                case ActiveDirectoryFieldOperator.EndsWith:
                    return entry.GetPropertyValue(customField.FieldName).ToString().EndsWith(andFilter.Value.ToString());
                case ActiveDirectoryFieldOperator.AfterNow:
                    var raw3 = entry.GetCustomProperty<object>(customField.FieldName);
                    var dateValue = raw3.AdsValueToDateTime(); if (dateValue is DateTime dateTime)
                    {
                        return dateTime > DateTime.Now;
                    }
                    break;
                case ActiveDirectoryFieldOperator.BeforeNow:
                    var raw4 = entry.GetCustomProperty<object>(customField.FieldName);
                    var dateValue2 = raw4.AdsValueToDateTime();
                    if (dateValue2 is DateTime dateTime2)
                    {
                        return dateTime2 < DateTime.Now;
                    }
                    break;
                case ActiveDirectoryFieldOperator.Contains:
                    var propertyValue = entry.GetPropertyValue(customField.FieldName).ToString();
                    return propertyValue?.Contains(andFilter.Value.ToString(), StringComparison.InvariantCultureIgnoreCase) == true;
                case ActiveDirectoryFieldOperator.Boolean:
                    if (entry.GetPropertyValue(customField.FieldName) is bool boolValue)
                    {
                        return boolValue == true;
                    }
                    break;
            }
            return false;
        }
        private static bool AndFilterTrue(AutomationRuleAndFilter andFilter, IDirectoryEntryAdapter entry)
        {
            var filterTrue = false;
            try
            {
                if (andFilter.CurrentField is ActiveDirectoryField defaultField)
                {
                    filterTrue = CheckDefaultFieldFilter(andFilter, entry, defaultField);
                }
                else if (andFilter.CurrentField is CustomActiveDirectoryField customField)
                {
                    filterTrue = CheckCustomFieldFilter(andFilter, entry, customField);
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
