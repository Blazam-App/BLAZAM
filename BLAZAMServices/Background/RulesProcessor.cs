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
using BLAZAM.Localization;
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

                    foreach (var ruleForEvent in applicableRules)
                    {
                        ProcessRule(ruleForEvent, args.Entry);

                    }
                }
            }
        }

        private void ProcessScheduledRule(AutomationRule rule)
        {
            using (var directory = activeDirectoryContextFactory.CreateActiveDirectoryContext()){
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
            var anyAndTrue = false;
            if (ruleForEvent.Filters.Count > 0)
            {
                foreach (var orFilter in ruleForEvent.Filters)
                {
                    var andTrue = true;
                    foreach (var andFilter in orFilter.AndFilters)
                    {
                        if (FilterTrue(andFilter, entry))
                        {
                            anyAndTrue = true;

                        }
                    }

                }
            }
            else
            {
                anyAndTrue = true;
            }

            return anyAndTrue;
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
                        if(target!= null)
                        {
                            entry.MoveTo(target);
                        }
                    }
                    break;
            }
            var result = entry.CommitChanges();

        }

        private bool FilterTrue(AutomationRuleAndFilter andFilter, IDirectoryEntryAdapter entry)
        {
            switch (andFilter.Operator)
            {
                case ActiveDirectoryFieldOperator.Equals:

                    return entry.PropertyValueEquals(andFilter.Field.DisplayName, andFilter.Value);

                case ActiveDirectoryFieldOperator.HistoricalTimeFrame:
                    var value = entry.GetPropertyValue(andFilter.Field.DisplayName);
                    break;
                case ActiveDirectoryFieldOperator.StartsWith:
                    var val = entry.GetPropertyValue(andFilter.Field.FieldName);
                    return val.ToString().Contains(andFilter.Value.ToString());


                case ActiveDirectoryFieldOperator.EndsWith:
                    break;
                case ActiveDirectoryFieldOperator.AfterNow:
                    break;
                case ActiveDirectoryFieldOperator.BeforeNow:
                    break;
                case ActiveDirectoryFieldOperator.Contains:
                    break;
                case ActiveDirectoryFieldOperator.FutureTimeFrame:
                    break;

            }
            return false;
        }

        private List<AutomationRule> GetRules()
        {
            using var context = dbFactory.CreateDbContext();
            return context.AutomationRules.Where(r => r.DeletedAt == null).ToList();
        }
    }
}
