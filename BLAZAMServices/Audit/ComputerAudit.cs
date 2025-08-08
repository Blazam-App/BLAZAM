using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Audit;
using BLAZAM.Helpers;
using BLAZAM.Logger;
using BLAZAM.Session.Interfaces;
using Microsoft.JSInterop;

namespace BLAZAM.Services.Audit
{
    public class ComputerAudit : DirectoryAudit
    {
        public ComputerAudit(IAppDatabaseFactory factory, IApplicationUserState? userState = null, IJSRuntime? jSRuntime = null) : base(factory, userState, jSRuntime)
        {
        }

        public async Task<bool> Moved(IDirectoryEntryAdapter movedComputer, IADOrganizationalUnit ouMovedFrom, IADOrganizationalUnit ouMovedTo)
        {
            Analytics?.ObjectMoved(ActiveDirectoryObjectType.Computer);

            await Log(c => c.DirectoryEntryAuditLogs,
               AuditActions.Computer_Moved,
            movedComputer,
               ouMovedFrom.OU,
               ouMovedTo.OU);
            return true;
        }
        public override async Task<bool> Changed(IDirectoryEntryAdapter changedComputer, List<AuditChangeLog> changes)
        {
            await Log(c => c.DirectoryEntryAuditLogs, AuditActions.Computer_Edited, changedComputer, changes.GetValueChangesString(c => c.OldValue), changes.GetValueChangesString(c => c.NewValue));
            return true;
        }

        public override async Task<bool> Deleted(IDirectoryEntryAdapter deletedEntry)
        {
            Analytics?.ObjectDeleted(ActiveDirectoryObjectType.Computer);

            return await Log(t => t.DirectoryEntryAuditLogs,
             AuditActions.Computer_Deleted, deletedEntry);
        }

        public async Task<bool> Assigned(IDirectoryEntryAdapter member, IDirectoryEntryAdapter parent)
        {
            Analytics?.ObjectAssigned(ActiveDirectoryObjectType.Computer);

            await Log(c => c.DirectoryEntryAuditLogs,
               AuditActions.Computer_Assigned,
            member,
               null,
               "Assigned to " + parent.DN);

            return true;
        }
        public async Task<bool> Unassigned(IDirectoryEntryAdapter member, IDirectoryEntryAdapter parent)
        {
            Analytics?.ObjectUnassigned(ActiveDirectoryObjectType.Computer);

            await Log(c => c.DirectoryEntryAuditLogs,
               AuditActions.Computer_Unassigned,
            member,
               null,
               "Unassigned from " + parent.DN);

            return true;
        }

        public override async Task<bool> Searched(IDirectoryEntryAdapter searchedEntry) => await Log(AuditActions.Computer_Searched, (IADComputer)searchedEntry);

        private async Task<bool> Log(string action, IADComputer searchedComputer)
        {

            try
            {
                using var context = await factory.CreateDbContextAsync();
                context.DirectoryEntryAuditLogs.Add(new ComputerAuditLog
                {
                    Sid = searchedComputer.SID.ToSidString(),
                    Action = action,
                    Target = searchedComputer.CanonicalName,
                    Username = CurrentUser?.AuditUsername,
                    IpAddress = CurrentUser?.IPAddress

                });
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Unable to write Log to database");

                return false;
            }
        }
    }
}