using BLAZAM.Common.Data;
using BLAZAM.Helpers;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Audit;

namespace BLAZAM.Services.Audit
{
    public class SystemAudit : BaseAudit
    {

        public SystemAudit(IAppDatabaseFactory factory) : base(factory)
        {
        }
        public async Task<bool> LogMessage(string message)
        {

            return await Log(message);
        }

        public async Task<bool> SettingsChanged(string category, List<AuditChangeLog> changes)
        {

            return await Log("Settings_Changed",
                changes.GetValueChangesString(c => c.OldValue),
                changes.GetValueChangesString(c => c.NewValue)
                );
        }


        private async Task<bool> Log(string action,
            string? beforeAction = null,
            string? afterAction = null)
        {
            try
            {
                using var context = await Factory.CreateDbContextAsync();
                context.SystemAuditLog.Add(new SystemAuditLog
                {
                    Action = action,
                    Username = "System",
                    BeforeAction = beforeAction,
                    AfterAction = afterAction,
                    Timestamp = DateTime.Now,



                });
                context.SaveChanges();
                return true;

            }
            catch
            {
                return false;
            }
        }
    }
}