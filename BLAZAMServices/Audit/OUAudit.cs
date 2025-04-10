using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Session.Interfaces;
using Microsoft.JSInterop;

namespace BLAZAM.Services.Audit
{
    public class OUAudit : DirectoryAudit
    {
        public OUAudit(IAppDatabaseFactory factory, IApplicationUserState? userState = null, IJSRuntime? jSRuntime = null) : base(factory, userState, jSRuntime)
        {
        }

        public async Task<bool> Moved(IDirectoryEntryAdapter movedOU, IADOrganizationalUnit ouMovedFrom, IADOrganizationalUnit ouMovedTo)
        {
            Analytics?.ObjectMoved(ActiveDirectoryObjectType.OU);
            await Log(c => c.DirectoryEntryAuditLogs,
               AuditActions.OU_Moved,
            movedOU,
               ouMovedFrom.OU,
               ouMovedTo.OU);
            return true;
        }
        public override async Task<bool> Deleted(IDirectoryEntryAdapter deletedEntry)
        {
            Analytics?.ObjectDeleted(ActiveDirectoryObjectType.OU);
            return await Log(t => t.DirectoryEntryAuditLogs,
             AuditActions.OU_Deleted, deletedEntry);

        }

        public override async Task<bool> Searched(IDirectoryEntryAdapter searchedOU)
            => await Log(c => c.DirectoryEntryAuditLogs,
                AuditActions.OU_Searched,
                searchedOU);

        public override async Task<bool> Created(IDirectoryEntryAdapter newOU)

        {
            Analytics?.ObjectCreated(ActiveDirectoryObjectType.OU);

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