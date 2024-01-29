using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Audit;
using BLAZAM.Session.Interfaces;
using System.Net;
using System.Security.Claims;

namespace BLAZAM.Services.Audit
{
    public class LogonAudit : CommonAudit
    {
        public LogonAudit(IAppDatabaseFactory factory,
            IApplicationUserStateService userStateService) : base(factory, userStateService)
        {
        }
        public async Task<bool> AttemptedPersonation(IPAddress iPAddress)
        {
            CurrentUser = UserStateService.CurrentUserState;
            return await Log("Attempted Personation", iPAddress);
        }

        public async Task<bool> AttemptedLogin(ClaimsPrincipal user, IPAddress iPAddress)
        {
            CurrentUser = UserStateService.CreateUserState(user);
            return await Log("Attempted Login", iPAddress);
        }
        public async Task<bool> Login(ClaimsPrincipal user,IPAddress? ipAddress=null)
        {
            CurrentUser = UserStateService.CreateUserState(user);
            return await Log("Login", ipAddress);
        }
        public async Task<bool> Logout() => await Log("Logout");

        private async Task<bool> Log(string action,IPAddress ipAddress=null)
        {

            try
            {
                using var context = await Factory.CreateDbContextAsync();
                var newAuditEntry = new LogonAuditLog
                {
                    Action = action,
                    Username = CurrentUser.AuditUsername,
                };
                if (ipAddress != null)
                    newAuditEntry.IpAddress = ipAddress.ToString();
                else
                    newAuditEntry.IpAddress = CurrentUser.IPAddress?.ToString();

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