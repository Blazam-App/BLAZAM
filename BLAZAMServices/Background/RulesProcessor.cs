using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Services;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Rules;
using BLAZAM.Localization;
using BLAZAM.Services.Events;
using Microsoft.Extensions.Localization;
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
        public RulesProcessor(IActiveDirectoryContextFactory activeDirectoryContextFactory, IAppDatabaseFactory dbFactory, IStringLocalizer<AppLocalization> appLocalization) : base(activeDirectoryContextFactory, dbFactory, appLocalization)
        {
            Interval = TimeSpan.Zero;
        }

        protected override void Execute(object? state = null)
        {
            ApplicationEvents.DirectoryEntryChanged.Delegate += ProcessDirectoryEntryChanged;
        }
        private void ProcessDirectoryEntryChanged(object? sender, DirectoryEntryChangedArgs args)
        {
            if (args.EventType != AppEventType.Search)
            {
                var rules = GetRules();
                if (rules.Count > 0)
                {
                    var applicableRules = rules
                        .Where(r => r.ActiveDirectoryObjectType.Equals(args.Entry.ObjectType)
                    && r.Enabled && r.Trigger.Equals(args.EventType.ToNotificationType()));
                    foreach (var ruleForEvent in applicableRules)
                    {
                        var anyAndTrue = false;
                        if (ruleForEvent.Filters.Count > 0)
                        {
                            foreach (var orFilter in ruleForEvent.Filters)
                            {
                                var andTrue = true;
                                foreach (var andFilter in orFilter.AndFilters)
                                {
                                    if (FilterTrue(andFilter, args.Entry))
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
                        if (anyAndTrue)
                        {
                            var test = "any is true";
                            //TODO Finish Rule Action Implementation
                        }


                    }
                }
            }
        }

        private bool FilterTrue(AutomationRuleAndFilter andFilter, IDirectoryEntryAdapter entry)
        {
            switch (andFilter.Operator)
            {
                case Database.Models.ActiveDirectoryFieldOperator.Equals:
                    var props = entry.GetType().GetProperties();
                    var matchingProp = props.FirstOrDefault(p => p.Name.Equals(andFilter.Field.DisplayName));
                    if (matchingProp.GetValue(entry).Equals(andFilter.Value))
                    {
                        return true;
                    }
                    break;
                case Database.Models.ActiveDirectoryFieldOperator.HistoricalTimeFrame:
                    break;
                case Database.Models.ActiveDirectoryFieldOperator.StartsWith:
                    break;
                case Database.Models.ActiveDirectoryFieldOperator.EndsWith:
                    break;
                case Database.Models.ActiveDirectoryFieldOperator.AfterNow:
                    break;
                case Database.Models.ActiveDirectoryFieldOperator.BeforeNow:
                    break;
                case Database.Models.ActiveDirectoryFieldOperator.Contains:
                    break;
                case Database.Models.ActiveDirectoryFieldOperator.FutureTimeFrame:
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
