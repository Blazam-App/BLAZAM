using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Helpers;
using BLAZAM.Session.Interfaces;
using Microsoft.JSInterop;

namespace BLAZAM.Services.Audit
{
    public class UserAudit : DirectoryAudit
    {
        public UserAudit(IAppDatabaseFactory factory, IApplicationUserState? userState = null, IJSRuntime? jSRuntime = null) : base(factory, userState, jSRuntime)
        {
        }

        public override async Task<bool> Deleted(IDirectoryEntryAdapter deletedEntry)
        {
            Analytics?.ObjectDeleted(ActiveDirectoryObjectType.User);

            return await Log(t => t.DirectoryEntryAuditLogs,
            AuditActions.User_Deleted, deletedEntry);
        }

        public override async Task<bool> Searched(IDirectoryEntryAdapter searchedUser)
            => await Log(c => c.DirectoryEntryAuditLogs,
                AuditActions.User_Searched,
                searchedUser);

        public async Task<bool> PasswordChanged(IDirectoryEntryAdapter searchedUser,
            bool requirePasswordChanged = false)
        {
            Analytics?.ObjectPasswordReset(ActiveDirectoryObjectType.User);

            return await Log(c => c.DirectoryEntryAuditLogs, AuditActions.Password_Changed, searchedUser, null, "requirePasswordChange=" + requirePasswordChanged);
        }

        public async Task<bool> Assigned(IDirectoryEntryAdapter member, IDirectoryEntryAdapter parent)
        {
            Analytics?.ObjectAssigned(ActiveDirectoryObjectType.User);

            await Log(c => c.DirectoryEntryAuditLogs,
               AuditActions.User_Assigned,
            member,
               null,
               "Assigned to " + parent.DN);

            return true;
        }
        public async Task<bool> Unassigned(IDirectoryEntryAdapter member, IDirectoryEntryAdapter parent)
        {
            Analytics?.ObjectUnassigned(ActiveDirectoryObjectType.User);

            await Log(c => c.DirectoryEntryAuditLogs,
               AuditActions.User_Unassigned,
            member,
               null,
               "Unassigned from " + parent.DN);

            return true;
        }

        public override async Task<bool> Created(IDirectoryEntryAdapter newUser)
        {
            Analytics?.ObjectCreated(ActiveDirectoryObjectType.User);

            var oldValues = "";
            var newValues = "";
            foreach (var c in newUser.NewEntryProperties)
            {
                newValues += c.Key + "=" + c.Value;
            }
            await Log(c => c.DirectoryEntryAuditLogs,
                AuditActions.User_Created,
                newUser,
                oldValues,
                newValues);
            return true;
        }
        public async Task<bool> Moved(IDirectoryEntryAdapter movedUser, IADOrganizationalUnit ouMovedFrom, IADOrganizationalUnit ouMovedTo)
        {
            Analytics?.ObjectMoved(ActiveDirectoryObjectType.User);

            await Log(c => c.DirectoryEntryAuditLogs,
               AuditActions.User_Moved,
            movedUser,
               ouMovedFrom.OU,
               ouMovedTo.OU);
            return true;
        }
        public override async Task<bool> Changed(IDirectoryEntryAdapter changedUser, List<AuditChangeLog> changes)
        {
            await Log(c => c.DirectoryEntryAuditLogs,
                AuditActions.User_Edited,
                changedUser,
                changes.GetValueChangesString(c => c.OldValue),
                changes.GetValueChangesString(c => c.NewValue));
            return true;
        }



    }
}