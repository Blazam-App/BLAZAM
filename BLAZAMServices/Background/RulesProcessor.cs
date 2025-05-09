using AngleSharp.Dom;
using BLAZAM.ActiveDirectory;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Searchers;
using BLAZAM.ActiveDirectory.Services;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Database.Models.Rules;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using BLAZAM.Localization;
using BLAZAM.Logger;
using BLAZAM.Services.Audit;
using BLAZAM.Services.Events;
using BLAZAM.Session;
using Microsoft.Extensions.Localization;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Services.Background
{
    [AutoStartBackgroundService(true)]
    public class RulesProcessor : ActiveDirectoryBackgroundServiceBase
    {
        private Dictionary<AutomationRule, Timer> ScheduledRules = new();
        private bool _initialized;

        private RulesAuditLogger Audit { get; set; }

        public RulesProcessor(IActiveDirectoryContextFactory activeDirectoryContextFactory, IAppDatabaseFactory dbFactory, IStringLocalizer<AppLocalization> appLocalization) : base(activeDirectoryContextFactory, dbFactory, appLocalization)
        {
            Interval = TimeSpan.FromMinutes(5);
        }

        protected override void Execute(object? state = null)
        {
            if (!_initialized)
            {
                ApplicationEvents.DirectoryEntryChanged.Delegate += ProcessDirectoryEntryChanged;
                _initialized = true;
            }
            ScheduleRules();
        }

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

                        if (timeToRun.Value < timeNow)
                        {
                            timeToRun = timeToRun.Value.Add(TimeSpan.FromDays(1));
                        }
                        var timeFromRun = timeToRun.Value - timeNow;
                        Timer ruleTimer = new Timer((state) => { ProcessScheduledRule(rule); }, null, (int)timeFromRun.TotalMilliseconds, Timeout.Infinite);
                        ScheduledRules.Add(rule, ruleTimer);
                    }
                }
            }
        }


        public override void Dispose()
        {
            base.Dispose();
            foreach (var scheduledRule in ScheduledRules)
            {
                scheduledRule.Value.Dispose();
            }
            ScheduledRules.Clear();
        }
        private void ProcessDirectoryEntryChanged(object? sender, DirectoryEntryChangedArgs args)
        {
            if (sender != null && sender.Equals(this)) return;
            if (args.EventType != ApplicationEventType.Search)
            {
                var rules = GetRules();
                if (rules.Count > 0)
                {
                    var applicableRules = rules
                        .Where(r => r.ActiveDirectoryObjectType.Equals(args.Entry.ObjectType)
                        && r.Trigger.Equals(args.EventType.ToNotificationType())).ToList();
                    var ruleProcessingJob = new Job();
                    ruleProcessingJob.StopOnFailedStep = true;
                    foreach (var ruleForEvent in applicableRules)
                    {
                        var ruleStep = new JobStep(ruleForEvent.Name, (step) =>
                        {
                            ProcessRule(ruleForEvent, args.Entry);
                            return true;
                        });
                        ruleProcessingJob.AddStep(ruleStep);
                    }
                    _ = ruleProcessingJob.RunAsync();
                }
            }
        }

        public void ProcessScheduledRule(AutomationRule rule)
        {
            Job scheduledRuleJob = new Job(AppLocalization[Lang.Scheduled_Rule], AppLocalization[Lang.Rules] + " " + rule.Name);
            List<IDirectoryEntryAdapter> filteredEntries = new();

            JobStep getApplicableEntriesStep = new("Get applicable entries", (step) =>
            {
                filteredEntries = GetFilteredEntries(rule);
                return true;
            });
            scheduledRuleJob.AddStep(getApplicableEntriesStep);
            scheduledRuleJob.StopOnFailedStep = true;
            JobStep execApplicableEntriesStep = new("Execute applicable entries", (step) =>
            {
                //Execute matched entries
                foreach (var entry in filteredEntries)
                {
                    ProcessRule(rule, entry);
                }
                return true;
            });

            scheduledRuleJob.AddStep(execApplicableEntriesStep);


            scheduledRuleJob.Run();
        }

        public List<IDirectoryEntryAdapter> GetFilteredEntries(AutomationRule rule)
        {
            List<IDirectoryEntryAdapter> matchedEntries = new();
            using (var directory = activeDirectoryContextFactory.CreateActiveDirectoryContext())
            {
                foreach(var orFilter in rule.Filters)
                {
                    ADSearch search = new ADSearch(directory);


                    //Perform search customization for this rule

                    //Has enabled filter
                    if (orFilter.AndFilters.Any(a => a.Field?.Equals(ActiveDirectoryFields.Enabled) == true && !a.Negate) &&
                        !orFilter.AndFilters.Any(a => a.Field?.Equals(ActiveDirectoryFields.Enabled) == true && a.Negate))
                    {
                        search.EnabledOnly = true;
                    }
                    else if (orFilter.AndFilters.Any(a => a.Field?.Equals(ActiveDirectoryFields.Enabled) == true && a.Negate) &&
                        !orFilter.AndFilters.Any(a => a.Field?.Equals(ActiveDirectoryFields.Enabled) == true && !a.Negate))
                    {
                        search.DisabledOnly = true;
                    }

                    //Has OU scope
                    if (orFilter.AndFilters.Any(a => a.Field?.Equals(ActiveDirectoryFields.OU) == true))
                    {
                        var ouFilters = orFilter.AndFilters.Where(a => a.Field?.Equals(ActiveDirectoryFields.OU) == true) // This part identifies the Filters containing the OU AndFilter
                            .Select(f => f.Value); // This flattens the collection of AndFilters from the matched Filters
                        var ouFilter = ouFilters.OrderBy(f => f.Length).FirstOrDefault();
                        if (ouFilter != null && ouFilter != null)
                        {
                            var ou = directory.OUs.FindOuByDN(ouFilter);
                            ou?.EnsureDirectoryEntry();
                            var ouDE = ou?.DirectoryEntry;
                            if (ouDE != null)
                            {
                                search.SearchRoot = ouDE;
                            }
                        }
                    }

                    if (rule.ActiveDirectoryObjectType != ActiveDirectoryObjectType.All)
                    {
                        search.ObjectTypeFilter = rule.ActiveDirectoryObjectType;
                    }






                    foreach (var andFilters in rule.Filters.Select(x => x.AndFilters))
                    {
                        foreach (var andFilter in andFilters)
                        {
                            if (andFilter.Field?.Equals(ActiveDirectoryFields.Enabled) == false
                                && andFilter.Field?.Equals(ActiveDirectoryFields.OU) == false)
                            {
                                PrepareADSearch(search, andFilter);
                            }

                        }
                    }
                    matchedEntries.AddRange(search.Search());
                }
                //ADSearch search = new ADSearch(directory);


                ////Perform search customization for this rule

                ////Has enabled filter
                //if (rule.Filters.Any(f => f.AndFilters.Any(a => a.Field?.Equals(ActiveDirectoryFields.Enabled) == true && !a.Negate)) &&
                //    !rule.Filters.Any(f => f.AndFilters.Any(a => a.Field?.Equals(ActiveDirectoryFields.Enabled) == true && a.Negate)))
                //{
                //    search.EnabledOnly = true;
                //}
                //else if (rule.Filters.Any(f => f.AndFilters.Any(a => a.Field?.Equals(ActiveDirectoryFields.Enabled) == true && a.Negate)) &&
                //    !rule.Filters.Any(f => f.AndFilters.Any(a => a.Field?.Equals(ActiveDirectoryFields.Enabled) == true && !a.Negate)))
                //{
                //    search.DisabledOnly = true;
                //}

                ////Has OU scope
                //if (rule.Filters.Any(f => f.AndFilters.Any(a => a.Field?.Equals(ActiveDirectoryFields.OU) == true)))
                //{
                //    var matchingAndFilters = rule.Filters
                //        .Where(f => f.AndFilters.Any(a => a.Field?.Equals(ActiveDirectoryFields.OU) == true)) // This part identifies the Filters containing the OU AndFilter
                //        .SelectMany(f => f.AndFilters) // This flattens the collection of AndFilters from the matched Filters
                //        .Where(a => a.Field?.Equals(ActiveDirectoryFields.OU) == true); // This selects the specific AndFilters with the OU field
                //    var ouFilter = matchingAndFilters.OrderBy(f => f.Value.Length).FirstOrDefault();
                //    if (ouFilter != null && ouFilter.Value!=null)
                //    {
                //        var ou = directory.OUs.FindOuByDN(ouFilter.Value);
                //        ou?.EnsureDirectoryEntry();
                //        var ouDE = ou?.DirectoryEntry;
                //        if (ouDE != null)
                //        {
                //            search.SearchRoot = ouDE;
                //        }
                //    }
                //}

                //if (rule.ActiveDirectoryObjectType != ActiveDirectoryObjectType.All)
                //{
                //    search.ObjectTypeFilter = rule.ActiveDirectoryObjectType;
                //}






                //foreach (var andFilters in rule.Filters.Select(x => x.AndFilters))
                //{
                //    foreach (var andFilter in andFilters)
                //    {
                //        if (andFilter.Field?.Equals(ActiveDirectoryFields.Enabled) == false
                //            && andFilter.Field?.Equals(ActiveDirectoryFields.OU) == false)
                //        {
                //            PrepareADSearch(search, andFilter);
                //        }

                //    }
                //}
                //matchedEntries = search.Search();


            }

            return matchedEntries;
        }

        private static void PrepareADSearch(ADSearch search, AutomationRuleAndFilter? andFilter)
        {
            try
            {
                var fieldValue = new ADFieldValue()
                {
                    Field = andFilter.CurrentField,
                    Value = andFilter.TimeFrame == null ? andFilter.Value : andFilter.TimeFrame,
                    Operator = andFilter.Operator,
                    Negate = andFilter.Negate
                };
                if (andFilter.CurrentField is ActiveDirectoryField defaultField)
                {
                    search.FieldValues.Add(fieldValue);
                }
                else if (andFilter.CurrentField is CustomActiveDirectoryField customField)
                {
                    search.FieldValues.Add(fieldValue);
                }
            }
            catch (Exception ex)
            {
                Loggers.RulesLogger.Warning("Unable to set search field value {@Field}{@Value}{@Error}", andFilter.Field, andFilter.Value, ex);
            }
        }

        private Job ProcessRule(AutomationRule? ruleForEvent, IDirectoryEntryAdapter? entry = null)
        {

            Job processRuleJob = new Job($"Run {ruleForEvent.Name} on {entry.CanonicalName}");

            JobStep executeRule = new("Execute", (step) =>
            {


                try
                {
                    using var context = dbFactory.CreateDbContext();
                    var contextRule = context.AutomationRules.First(r => r.Id.Equals(ruleForEvent.Id));
                    contextRule.LastTriggered = DateTime.UtcNow;
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Loggers.RulesLogger.Error("Error while setting LastTriggered for rule {@Rule}{@Error}", ruleForEvent, ex);
                }
                if (OrFiltersPass(ruleForEvent, entry))
                {
                    try
                    {
                        using var context = dbFactory.CreateDbContext();
                        var contextRule = context.AutomationRules.First(r => r.Id.Equals(ruleForEvent.Id));
                        contextRule.LastExcecuted = DateTime.UtcNow;
                        context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Loggers.RulesLogger.Error("Error while setting LastExcecuted for rule {@Rule}{@Error}", ruleForEvent, ex);
                    }
                    foreach (var action in ruleForEvent.Actions)
                    {
                        try
                        {
                            ExecuteAction(ruleForEvent, action, entry);
                        }
                        catch (Exception ex)
                        {
                            Loggers.RulesLogger.Error("Error while executing rule action. {@Rule}{@TargetDN}{@Action}{@Error}", ruleForEvent, entry, action, ex);
                            break;
                        }
                    }
                    Loggers.RulesLogger.Debug("Processing for rule {@Rule} has finished", ruleForEvent);

                    if (ruleForEvent.StopOnThisRule)
                    {
                        return false;
                    }
                }
                return true;
            });
            processRuleJob.AddStep(executeRule);
            _ = processRuleJob.RunAsync();
            return processRuleJob;
        }

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

        private void ExecuteAction(AutomationRule rule, AutomationRuleAction action, IDirectoryEntryAdapter entry)
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
                        if (entry.GetParent().DN.Equals(action.Data, StringComparison.InvariantCultureIgnoreCase)) return;

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
                        if (existingValue.ToString().Equals(action.FieldValues[0].Value, StringComparison.InvariantCultureIgnoreCase)) return;
                        entry.SetCustomProperty(field.FieldName, action.FieldValues[0].Value);
                    }
                    break;
            }
            var changes = entry.Changes;
            var result = entry.CommitChanges();
            if (result.FailedSteps.Count == 0)
            {
                Audit = new(dbFactory, new RulesUserState(dbFactory, rule.Name));

                ApplicationEvents.DirectoryEntryChanged.Invoke(this, new()
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
                Loggers.RulesLogger.Error("Error checking and filter {@Filter}{@Error}", andFilter, ex);
            }
            if (andFilter.Negate)
            {
                filterTrue = !filterTrue;
            }
            return filterTrue;
        }

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
                Loggers.RulesLogger.Debug("Error loading rules {@Error}", ex);
                return [];
            }
        }
    }
}
