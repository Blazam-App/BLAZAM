using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Services.Audit
{
    public class OUAudit : DirectoryAudit
    {
        public OUAudit(IAppDatabaseFactory factory,
            IApplicationUserStateService userStateService) : base(factory, userStateService)
        {
        }
        public async Task<bool> Moved(IDirectoryEntryAdapter movedOU, IADOrganizationalUnit ouMovedFrom, IADOrganizationalUnit ouMovedTo)
        {
            await Log(c => c.DirectoryEntryAuditLogs,
               AuditActions.OU_Moved,
            movedOU,
               ouMovedFrom.OU,
               ouMovedTo.OU);
            return true;
        }
        public override async Task<bool> Deleted(IDirectoryEntryAdapter deletedEntry)
         => await Log(t => t.DirectoryEntryAuditLogs,
             AuditActions.OU_Deleted, deletedEntry);


        public override async Task<bool> Searched(IDirectoryEntryAdapter searchedOU)
            => await Log(c => c.DirectoryEntryAuditLogs,
                AuditActions.OU_Searched,
                searchedOU);

        public override async Task<bool> Created(IDirectoryEntryAdapter newOU)

        {
            var newValues = "";
            foreach (var c in newOU.NewEntryProperties)
            {
                newValues += c.Key + "=" + c.Value;
            }
            await Log(c => c.DirectoryEntryAuditLogs, AuditActions.OU_Created, newOU, "", newValues);
            return true;
        }

    }
}