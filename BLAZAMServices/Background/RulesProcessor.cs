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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Services.Background
{
    [AutoStartBackgroundService(true)]
    public class RulesProcessor : ActiveDirectoryBackgroundServiceBase
    {
        private List<Timer> ScheduledRules = new();

        private RulesAuditLogger Audit { get; set; }

        public RulesProcessor(IActiveDirectoryContextFactory activeDirectoryContextFactory, IAppDatabaseFactory dbFactory, IStringLocalizer<AppLocalization> appLocalization) : base(activeDirectoryContextFactory, dbFactory, appLocalization)
        {
            Interval = TimeSpan.Zero;
        }

        protected override void Execute(object? state = null)
        {
            ApplicationEvents.DirectoryEntryChanged.Delegate += ProcessDirectoryEntryChanged;
            ScheduleRules();
        }

        private void ScheduleRules()
        {
            var rules = GetRules();
            var scheduledRules = rules.Where(r => r.Enabled && r.ScheduledRunTime != null && r.Trigger == NotificationType.Scheduled).ToList();
            foreach (var rule in scheduledRules)
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
                    ScheduledRules.Add(ruleTimer);
                }
            }
        }


        public override void Dispose()
        {
            base.Dispose();
            foreach (var timer in ScheduledRules)
            {
                timer.Dispose();
            }
            ScheduledRules.Clear();
        }
        private void ProcessDirectoryEntryChanged(object? sender, DirectoryEntryChangedArgs args)
        {
            if (args.EventType != ApplicationEventType.Search)
            {
                var rules = GetRules();
                if (rules.Count > 0)
                {
                    var applicableRules = rules
                        .Where(r => r.ActiveDirectoryObjectType.Equals(args.Entry.ObjectType)
                        && r.Trigger.Equals(args.EventType.ToNotificationType())).ToList();
                    var ruleProcessingJob = new Job();
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
            List<IDirectoryEntryAdapter> filteredEntries = new();
            filteredEntries = GetFilteredEntries(rule);

            //Execute matched entries
            foreach (var entry in filteredEntries)
            {
                ProcessRule(rule, entry);
            }
        }

        public List<IDirectoryEntryAdapter> GetFilteredEntries(AutomationRule rule)
        {
            List<IDirectoryEntryAdapter> matchedEntries;
            using (var directory = activeDirectoryContextFactory.CreateActiveDirectoryContext())
            {
                ADSearch search = new ADSearch(directory);


                //Perform search customization for this rule

                //Has enabled filter
                if (rule.Filters.Any(f => f.AndFilters.Any(a => a.Field.Equals(ActiveDirectoryFields.Enabled) && !a.Negate)) &&
                    !rule.Filters.Any(f => f.AndFilters.Any(a => a.Field.Equals(ActiveDirectoryFields.Enabled) && a.Negate)))
                {
                    search.EnabledOnly = true;
                }
                foreach (var andFilters in rule.Filters.Select(x => x.AndFilters))
                {
                    foreach (var andFilter in andFilters)
                    {
                        PrepareADSearch(search, andFilter);

                    }
                }
                matchedEntries = search.Search();


            }

            return matchedEntries;
        }

        private static void PrepareADSearch(ADSearch search, AutomationRuleAndFilter? andFilter)
        {
            try
            {
                var fieldValue = new ADFieldValue()
                {
                    Field = andFilter.Field,
                    Value = andFilter.Value,
                    Operator = andFilter.Operator,
                };
                switch (andFilter.Field.FieldType)
                {
                    case ActiveDirectoryFieldType.Text:
                        search.Fields.SetPropertyValue(andFilter.Field.PropertyName, andFilter.Value);
                        break;
                    case ActiveDirectoryFieldType.Date:
                        if (andFilter.Value != null)
                        {
                            search.Fields.SetPropertyValue(andFilter.Field.PropertyName, DateTime.Parse(andFilter.Value));
                            fieldValue.Value = DateTime.Parse(andFilter.Value);
                        }

                        break;
                    case ActiveDirectoryFieldType.RawData:
                        search.Fields.SetPropertyValue(andFilter.Field.PropertyName, andFilter.Value);
                        break;

                    case ActiveDirectoryFieldType.FileTime:
                        if (andFilter.Value != null)
                        {
                            search.Fields.SetPropertyValue(andFilter.Field.PropertyName, DateTime.FromFileTimeUtc(long.Parse(andFilter.Value)));
                            fieldValue.Value = DateTime.FromFileTimeUtc(long.Parse(andFilter.Value));
                        }
                        break;
                    case ActiveDirectoryFieldType.StringList: break;
                    case ActiveDirectoryFieldType.DriveLetter:
                        search.Fields.SetPropertyValue(andFilter.Field.PropertyName, andFilter.Value);
                        break;
                    case ActiveDirectoryFieldType.Boolean:
                        fieldValue.Value = "";
                        break;
                }
                search.FieldValues.Add(fieldValue);

            }
            catch (Exception ex)
            {
                Loggers.RulesLogger.Warning("Unable to set search field value {@Field}{@Value}{@Error}", andFilter.Field, andFilter.Value, ex);
            }
        }

        private void ProcessRule(AutomationRule? ruleForEvent, IDirectoryEntryAdapter? entry = null)
        {
            try
            {
                using var context = dbFactory.CreateDbContext();
                var contextRule = context.AutomationRules.First(r => r.Id.Equals(ruleForEvent.Id));
                contextRule.LastTriggered = DateTime.UtcNow;
                context.SaveChanges();
            }catch(Exception ex)
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

            }
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
            var account = entry as IAccountDirectoryAdapter;
            switch (action.ActionType)
            {
                case AutomationRuleActionType.Disable:
                    eventType = ApplicationEventType.Modify;
                    if (account != null)
                    {
                        account.Enabled = false;

                    }
                    break;
                case AutomationRuleActionType.Enable:
                    eventType = ApplicationEventType.Modify;

                    if (account != null)
                    {
                        account.Enabled = true;
                    }
                    break;
                case AutomationRuleActionType.Unlock:
                    eventType = ApplicationEventType.Modify;

                    if (account != null)
                    {
                        account.LockedOut = false;
                    }
                    break;
                case AutomationRuleActionType.Lockout:
                    eventType = ApplicationEventType.LockedOut;
                    if (account != null)
                    {
                        account.LockedOut = true;
                    }
                    break;
                case AutomationRuleActionType.Move:
                    eventType = ApplicationEventType.Modify;

                    if (action.Data != null)
                    {
                        using var directory = activeDirectoryContextFactory.CreateActiveDirectoryContext();
                        var target = directory.OUs.FindOuByDN(action.Data);
                        if (target != null)
                        {
                            entry.MoveTo(target);
                        }
                    }
                    break;
            }
            var changes = entry.Changes;
            var result = entry.CommitChanges();
            if (result.FailedSteps.Count == 0)
            {
                Audit = new(dbFactory, new RulesUserState(dbFactory,rule.Name));
                //Audit.User.Changed(entry, changes);
                Audit.ProcessDirectoryEntryChangedEvent(new DirectoryEntryChangedArgs()
                {
                    Actor = new RulesUserState(dbFactory,rule.Name),
                    Changes = changes,
                    Entry = entry,
                    EventType = eventType
                });
            }
        }

        private static bool AndFilterTrue(AutomationRuleAndFilter andFilter, IDirectoryEntryAdapter entry)
        {
            var filterTrue = false;
            try
            {
                switch (andFilter.Operator)
                {
                    case ActiveDirectoryFieldOperator.Equals:
                        filterTrue = entry.PropertyValueEquals(andFilter.Field.DisplayName, andFilter.Value);
                        break;

                    case ActiveDirectoryFieldOperator.HistoricalTimeFrame:
                        var dateValue3 = entry.GetPropertyValue(andFilter.Field.PropertyName);
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
                        var dateValue4 = entry.GetPropertyValue(andFilter.Field.PropertyName);
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
                        filterTrue = entry.GetPropertyValue(andFilter.Field.FieldName).ToString().StartsWith(andFilter.Value.ToString());
                        break;

                    case ActiveDirectoryFieldOperator.EndsWith:
                        filterTrue = entry.GetPropertyValue(andFilter.Field.FieldName).ToString().EndsWith(andFilter.Value.ToString());
                        break;

                    case ActiveDirectoryFieldOperator.AfterNow:
                        var dateValue = entry.GetPropertyValue(andFilter.Field.PropertyName);
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
                        var dateValue2 = entry.GetPropertyValue(andFilter.Field.PropertyName);
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
                        filterTrue = entry.GetPropertyValue(andFilter.Field.FieldName).ToString().Contains(andFilter.Value.ToString());
                        break;

                    case ActiveDirectoryFieldOperator.Boolean:
                        if (entry.GetPropertyValue(andFilter.Field.PropertyName) is bool boolValue)
                        {
                            filterTrue = boolValue == true;
                        }
                        break;



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
            using var context = dbFactory.CreateDbContext();
            return context.AutomationRules.Where(r => r.DeletedAt == null && r.Enabled == true && (r.ExpirationDate == null || r.ExpirationDate > DateTime.Now)).ToList();
        }
    }
}
