using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Audit;
using BLAZAM.Session.Interfaces;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace BLAZAM.Services.Audit
{
    public class LogonAudit : CommonAudit
    {
        public LogonAudit(IAppDatabaseFactory factory, IJSRuntime jSRuntime, IApplicationUserStateService userStateService) : base(factory, jSRuntime, userStateService)
        {
        }

        public async Task<bool> AttemptedPersonation(string? iPAddress = null)
        {
            CurrentUser = UserStateService.CurrentUserState;
            return await Log("Attempted Personation", iPAddress);
        }

        public async Task<bool> AttemptedLogin(ClaimsPrincipal user, string? iPAddress = null)
        {
            CurrentUser = UserStateService.CreateUserState(user);
            return await Log("Attempted Login", iPAddress);
        }
        public async Task<bool> Impersonate(ClaimsPrincipal impersonator, ClaimsPrincipal impersonateee, string? ipAddress = null)
        {
            CurrentUser = UserStateService.CreateUserState(impersonateee);
            CurrentUser.Impersonator = impersonator;
            return await Log("Impersonation", ipAddress);
        }
        public async Task<bool> Login(ClaimsPrincipal user, string? ipAddress = null)
        {
            CurrentUser = UserStateService.CreateUserState(user);
            return await Log("Login", ipAddress);
        }
        public async Task<bool> Logout() => await Log("Logout");

        private async Task<bool> Log(string action, string ipAddress = null)
        {

            try
            {
                using var context = await factory.CreateDbContextAsync();
                var newAuditEntry = new LogonAuditLog
                {
                    Action = action,
                    Username = CurrentUser.AuditUsername,
                };
                if (ipAddress != null)
                    newAuditEntry.IpAddress = ipAddress;
                else
                    newAuditEntry.IpAddress = CurrentUser.IPAddress;

                context.LogonAuditLog.Add(newAuditEntry);
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