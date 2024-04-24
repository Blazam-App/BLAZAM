using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Helpers;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Audit;
using BLAZAM.Session.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Services.Audit
{
    public class DirectoryAudit : CommonAudit
    {
        public DirectoryAudit(IAppDatabaseFactory factory, IApplicationUserStateService userStateService) : base(factory, userStateService)
        {
        }

        public virtual Task<bool> Changed(IDirectoryEntryAdapter changedUser, List<AuditChangeLog> changes)
        {
            throw new NotImplementedException();
        }
        public virtual async Task<bool> Deleted(IDirectoryEntryAdapter deletedEntry)
        {
            throw new NotImplementedException();

        }
        public virtual Task<bool> Created(IDirectoryEntryAdapter newUser)
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> Searched(IDirectoryEntryAdapter searchedUser)
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
                using var context = await Factory.CreateDbContextAsync();
                var table = auditTable.Invoke(context);
                var auditEntry = new T()
                {
                    Action = action,
                    Target = relatedEntry.CanonicalName,
                    Sid = relatedEntry.SID.ToSidString(),
                    BeforeAction = beforeAction,
                    AfterAction = afterAction,
                    Username = CurrentUser.AuditUsername,
                    IpAddress = CurrentUser.IPAddress,
                };
                table.Add(auditEntry);
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