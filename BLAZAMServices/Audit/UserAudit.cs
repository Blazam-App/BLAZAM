using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Helpers;
using BLAZAM.Database.Context;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Services.Audit
{
    public class UserAudit : DirectoryAudit
    {
        public UserAudit(IAppDatabaseFactory factory, IApplicationUserStateService userStateService) : base(factory, userStateService)
        {
        }

        public override async Task<bool> Deleted(IDirectoryEntryAdapter deletedEntry)
            => await Log(t => t.DirectoryEntryAuditLogs,
                AuditActions.User_Deleted, deletedEntry);

        public override async Task<bool> Searched(IDirectoryEntryAdapter searchedUser)
            => await Log(c => c.DirectoryEntryAuditLogs,
                AuditActions.User_Searched,
                searchedUser);

        public async Task<bool> PasswordChanged(IDirectoryEntryAdapter searchedUser,
            bool requirePasswordChanged = false)
            => await Log(c => c.DirectoryEntryAuditLogs, AuditActions.Password_Changed, searchedUser, null, "requirePasswordChange=" + requirePasswordChanged);

        public async Task<bool> Assigned(IDirectoryEntryAdapter member, IDirectoryEntryAdapter parent)
        {
            await Log(c => c.DirectoryEntryAuditLogs,
               AuditActions.User_Assigned,
            member,
               null,
               "Assigned to " + parent.DN);
           
            return true;
        }
         public async Task<bool> Unassigned(IDirectoryEntryAdapter member, IDirectoryEntryAdapter parent)
        {
            await Log(c => c.DirectoryEntryAuditLogs,
               AuditActions.User_Unassigned,
            member,
               null,
               "Unassigned from " + parent.DN);
           
            return true;
        }

        public override async Task<bool> Created(IDirectoryEntryAdapter newUser)
        {
            var oldValues = "";
            var newValues = "";
            foreach (var c in newUser.NewEntryProperties)
            {
                newValues += c.Key + "=" + c.Value;
            }
            await Log(c => c.DirectoryEntryAuditLogs, AuditActions.User_Created, newUser, oldValues, newValues);
            return true;
        }

        public override async Task<bool> Changed(IDirectoryEntryAdapter changedUser, List<AuditChangeLog> changes)
        {
            await Log(c => c.DirectoryEntryAuditLogs, AuditActions.User_Edited, changedUser, changes.GetValueChangesString(c => c.OldValue), changes.GetValueChangesString(c => c.NewValue));
            return true;
        }



    }
}