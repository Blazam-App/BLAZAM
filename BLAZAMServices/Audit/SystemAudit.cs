using BLAZAM.Database.Models.Audit;
using BLAZAM.Helpers;
using Microsoft.JSInterop;
using System.Net;

namespace BLAZAM.Services.Audit
{
    public class SystemAudit : BaseAudit
    {
        public SystemAudit(IAppDatabaseFactory factory, IJSRuntime? jSRuntime = null) : base(factory, jSRuntime)
        {
        }
        public async Task<bool> APITokenCreated(string username)
        {
            return await Log("API_Token_Created", null, null, username);
        }
        public async Task<bool> SettingsChanged(string category, List<AuditChangeLog> changes)
        {

            return await Log("Settings_Changed",
                changes.GetValueChangesString(c => c.OldValue),
                changes.GetValueChangesString(c => c.NewValue)
                );
        }

        public async Task<bool> PasswordResetRequested(string? ipAddress, string? username)
        {
            string afterAction = ipAddress != null ? $"IP Address: {ipAddress}" : "IP Address: Unknown";
            return await Log("Password_Reset_Requested", null, afterAction, username);
        }
        private async Task<bool> Log(string action,

            string? beforeAction = null,
            string? afterAction = null,
            string username = "System")
        {
            try
            {
                using var context = await factory.CreateDbContextAsync();
                context.SystemAuditLog.Add(new SystemAuditLog
                {
                    Action = action,
                    Username = username,
                    BeforeAction = beforeAction,
                    AfterAction = afterAction,
                    Timestamp = DateTime.UtcNow,



                });
                await context.SaveChangesAsync();
                return true;

            }
            catch
            {
                return false;
            }
        }
    }
}