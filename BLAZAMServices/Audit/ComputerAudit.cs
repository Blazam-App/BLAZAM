using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Audit;
using BLAZAM.Session.Interfaces;
using BLAZAM.Logger;
using BLAZAM.Helpers;
using BLAZAM.Common.Data;

namespace BLAZAM.Services.Audit
{
    public class ComputerAudit : DirectoryAudit
    {
        public ComputerAudit(IAppDatabaseFactory factory,
            IApplicationUserStateService userStateService) : base(factory, userStateService)
        {
        }
        public async Task<bool> Moved(IDirectoryEntryAdapter movedComputer, IADOrganizationalUnit ouMovedFrom, IADOrganizationalUnit ouMovedTo)
        {
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
         => await Log(t => t.DirectoryEntryAuditLogs,
             AuditActions.Computer_Deleted, deletedEntry);

        public async Task<bool> Assigned(IDirectoryEntryAdapter member, IDirectoryEntryAdapter parent)
        {
            await Log(c => c.DirectoryEntryAuditLogs,
               AuditActions.Computer_Assigned,
            member,
               null,
               "Assigned to " + parent.DN);

            return true;
        }
        public async Task<bool> Unassigned(IDirectoryEntryAdapter member, IDirectoryEntryAdapter parent)
        {
            await Log(c => c.DirectoryEntryAuditLogs,
               AuditActions.Computer_Unassigned,
            member,
               null,
               "Unassigned from " + parent.DN);

            return true;
        }

        public override async Task<bool> Searched(IDirectoryEntryAdapter searchedComputer) => await Log(AuditActions.Computer_Searched, (IADComputer)searchedComputer);

        private async Task<bool> Log(string action, IADComputer searchedComputer)
        {

            try
            {
                using var context = await Factory.CreateDbContextAsync();
                context.DirectoryEntryAuditLogs.Add(new ComputerAuditLog
                {
                    Sid = searchedComputer.SID.ToSidString(),
                    Action = action,
                    Target = searchedComputer.CanonicalName,
                    Username = UserStateService?.CurrentUsername,
                    IpAddress = UserStateService?.CurrentUserState?.IPAddress

                });
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error("Unable to write Log to database {@Error}", ex);

                return false;
            }
        }
    }
}