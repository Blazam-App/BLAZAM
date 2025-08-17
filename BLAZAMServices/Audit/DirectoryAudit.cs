using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Database.Interfaces;
using BLAZAM.Database.Models.Audit;
using BLAZAM.Helpers;
using BLAZAM.Session.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace BLAZAM.Services.Audit
{
    public class DirectoryAudit : CommonAudit
    {
        public DirectoryAudit(IAppDatabaseFactory factory, IApplicationUserState userState, IJSRuntime? jSRuntime = null) : base(factory, userState, jSRuntime)
        {
        }

        public virtual Task<bool> Changed(IDirectoryEntryAdapter changedEntry, List<AuditChangeLog> changes)
        {
            throw new NotImplementedException();
        }
        public virtual Task<bool> Deleted(IDirectoryEntryAdapter deletedEntry)
        {
            throw new NotImplementedException();

        }
        public virtual Task<bool> Created(IDirectoryEntryAdapter newEntry)
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> Searched(IDirectoryEntryAdapter searchedEntry)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">The Log Entry Type to insert</typeparam>
        /// <param name="auditTable"></param>
        /// <param name="action"></param>
        /// <param name="relatedEntry"></param>
        /// <param name="beforeAction"></param>
        /// <param name="afterAction"></param>
        /// <returns></returns>
        protected virtual async Task<bool> Log<T>(
            Func<IDatabaseContext,
            DbSet<T>> auditTable,
            string action,
            IDirectoryEntryAdapter relatedEntry,
            string? beforeAction = null,
            string? afterAction = null) where T : class, IDirectoryEntryAuditLog, new()
        {

            try
            {
                using var context = await factory.CreateDbContextAsync();
                var table = auditTable.Invoke(context);
                var username = UserState?.AuditUsername ?? CurrentUser?.AuditUsername ?? string.Empty;
                var auditEntry = new T()
                {
                    Action = action,
                    Target = relatedEntry.CanonicalName,
                    Sid = relatedEntry.SID.ToSidString(),
                    BeforeAction = beforeAction,
                    AfterAction = afterAction,
                    Username = username,
                    IpAddress = UserState?.IPAddress,
                };
                table.Add(auditEntry);
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