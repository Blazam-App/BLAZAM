using System.Security.Claims;
using BLAZAM.Database.Models.Audit;
using BLAZAM.Session;
using BLAZAM.Session.Interfaces;
using Microsoft.JSInterop;

namespace BLAZAM.Services.Audit
{
    public class LogonAudit : CommonAudit
    {
        public LogonAudit(IAppDatabaseFactory factory, IApplicationUserState? userState = null, IJSRuntime? jSRuntime = null) : base(factory, userState, jSRuntime)
        {
        }

        public async Task<bool> AttemptedPersonation(string? iPAddress = null)
        {
            return await Log("Attempted Personation", iPAddress);
        }

        public async Task<bool> AttemptedLogin(ClaimsPrincipal user, string? iPAddress = null)
        {
            UserState = ApplicationUserState.CreateUserState(user, factory);
            return await Log("Attempted Login", iPAddress);
        }
        public async Task<bool> Impersonate(ClaimsPrincipal impersonator, ClaimsPrincipal impersonateee, string? ipAddress = null)
        {
            UserState = ApplicationUserState.CreateUserState(impersonateee, factory);
            if (UserState != null)
            {
                UserState.Impersonator = impersonator;
            }
            return await Log("Impersonation", ipAddress);
        }
        public async Task<bool> Login(ClaimsPrincipal user, string? ipAddress = null)
        {
            UserState = ApplicationUserState.CreateUserState(user, factory);
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
                    Username = UserState?.AuditUsername,
                };
                if (ipAddress != null)
                    newAuditEntry.IpAddress = ipAddress;
                else
                    newAuditEntry.IpAddress = UserState?.IPAddress;

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