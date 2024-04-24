using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Services.Audit
{
    public class PrinterAudit : DirectoryAudit
    {
        public PrinterAudit(IAppDatabaseFactory factory,
            IApplicationUserStateService userStateService) : base(factory, userStateService)
        {
        }

        public override async Task<bool> Deleted(IDirectoryEntryAdapter deletedEntry)
         => await Log(t => t.DirectoryEntryAuditLogs,
             AuditActions.Printer_Deleted, deletedEntry);


        public override async Task<bool> Searched(IDirectoryEntryAdapter searchedOU)
            => await Log(c => c.DirectoryEntryAuditLogs,
                AuditActions.Printer_Searched,
                searchedOU);

        public override async Task<bool> Created(IDirectoryEntryAdapter newOU)

        {
            var oldValues = "";
            var newValues = "";
            foreach (var c in newOU.NewEntryProperties)
            {
                newValues += c.Key + "=" + c.Value;
            }
            await Log(c => c.DirectoryEntryAuditLogs, AuditActions.Printer_Created, newOU, oldValues, newValues);
            return true;
        }

    }
}