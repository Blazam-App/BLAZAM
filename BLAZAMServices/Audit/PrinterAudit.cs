using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Session.Interfaces;
using Microsoft.JSInterop;

namespace BLAZAM.Services.Audit
{
    public class PrinterAudit : DirectoryAudit
    {
        public PrinterAudit(IAppDatabaseFactory factory, IApplicationUserState? userState = null, IJSRuntime? jSRuntime = null) : base(factory, userState, jSRuntime)
        {
        }

        public async Task<bool> Moved(IDirectoryEntryAdapter movedPrinter, IADOrganizationalUnit ouMovedFrom, IADOrganizationalUnit ouMovedTo)
        {
            Analytics?.ObjectMoved(ActiveDirectoryObjectType.Printer);

            await Log(c => c.DirectoryEntryAuditLogs,
               AuditActions.Printer_Moved,
            movedPrinter,
               ouMovedFrom.OU,
               ouMovedTo.OU);
            return true;
        }
        public override async Task<bool> Deleted(IDirectoryEntryAdapter deletedEntry)
         => await Log(t => t.DirectoryEntryAuditLogs,
             AuditActions.Printer_Deleted, deletedEntry);


        public override async Task<bool> Searched(IDirectoryEntryAdapter searchedEntry)
            => await Log(c => c.DirectoryEntryAuditLogs,
                AuditActions.Printer_Searched,
                searchedEntry);

        public override async Task<bool> Created(IDirectoryEntryAdapter newEntry)

        {
            var oldValues = "";
            var newValues = "";
            foreach (var c in newEntry.NewEntryProperties)
            {
                newValues += c.Key + "=" + c.Value;
            }
            await Log(c => c.DirectoryEntryAuditLogs, AuditActions.Printer_Created, newEntry, oldValues, newValues);
            return true;
        }

    }
}