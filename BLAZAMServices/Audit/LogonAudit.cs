using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Audit;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Services.Audit
{
    public class LogonAudit : CommonAudit
    {
        public LogonAudit(IAppDatabaseFactory factory,
            IApplicationUserStateService userStateService) : base(factory, userStateService)
        {
        }

        public async Task<bool> Login(System.Security.Claims.ClaimsPrincipal user)
        {
            CurrentUser = UserStateService.CreateUserState(user);
            return await Log("Login");
        }
        public async Task<bool> Logout() => await Log("Logout");

        private async Task<bool> Log(string action)
        {

            try
            {
                using var context = await Factory.CreateDbContextAsync();
                context.LogonAuditLog.Add(new LogonAuditLog
                {
                    Action = action,
                    Username = CurrentUser.AuditUsername,
                    IpAddress = CurrentUser.IPAddress?.ToString(),
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