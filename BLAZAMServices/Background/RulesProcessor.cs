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

        public RulesAuditLogger Audit { get; }

        public RulesProcessor(RulesAuditLogger audit, IActiveDirectoryContextFactory activeDirectoryContextFactory, IAppDatabaseFactory dbFactory, IStringLocalizer<AppLocalization> appLocalization) : base(activeDirectoryContextFactory, dbFactory, appLocalization)
        {
            Interval = TimeSpan.Zero;
            Audit = audit;
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
                        timeToRun.Value.Add(TimeSpan.FromDays(1));
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
                    && r.Enabled && r.Trigger.Equals(args.EventType.ToNotificationType())).ToList();
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
                    _=ruleProcessingJob.RunAsync();
                }
            }
        }

        private void ProcessScheduledRule(AutomationRule rule)
        {
            using (var directory = activeDirectoryContextFactory.CreateActiveDirectoryContext())
            {
                ADSearch search = new ADSearch(directory);
                var results = search.Search();
                foreach (var entry in results)
                {
                    if (FiltersPass(rule, entry))
                    {
                        foreach (var action in rule.Actions)
                        {
                            ExecuteAction(action, entry);

                        }
                    }
                }
            }
        }
        private void ProcessRule(AutomationRule? ruleForEvent, IDirectoryEntryAdapter? entry = null)
        {
            if (FiltersPass(ruleForEvent, entry))
            {
                foreach (var action in ruleForEvent.Actions)
                {

                    ExecuteAction(action, entry);

                }
            }
        }

        private bool FiltersPass(AutomationRule? ruleForEvent, IDirectoryEntryAdapter? entry = null)
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

        private void ExecuteAction(AutomationRuleAction action, IDirectoryEntryAdapter entry)
        {

            var account = entry as IAccountDirectoryAdapter;
            switch (action.ActionType)
            {
                case AutomationRuleActionType.Disable:
                    if (account != null)
                    {
                        account.Enabled = false;
                    }
                    break;
                case AutomationRuleActionType.Enable:
                    if (account != null)
                    {
                        account.Enabled = true;
                    }
                    break;
                case AutomationRuleActionType.Unlock:
                    if (account != null)
                    {
                        account.LockedOut = false;
                    }
                    break;
                case AutomationRuleActionType.Lockout:
                    if (account != null)
                    {
                        account.LockedOut = true;
                    }
                    break;
                case AutomationRuleActionType.Move:
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
                Audit.User.Changed(entry, changes);
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
            return context.AutomationRules.Where(r => r.DeletedAt == null).ToList();
        }
    }
}
