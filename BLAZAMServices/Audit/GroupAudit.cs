using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Helpers;
using BLAZAM.Session.Interfaces;
using Microsoft.JSInterop;

namespace BLAZAM.Services.Audit
{
    public class GroupAudit : DirectoryAudit
    {
        public GroupAudit(IAppDatabaseFactory factory, IApplicationUserState? userState = null, IJSRuntime? jSRuntime = null) : base(factory, userState, jSRuntime)
        {
        }

        public override async Task<bool> Deleted(IDirectoryEntryAdapter deletedEntry)
        {
            Analytics?.ObjectDeleted(ActiveDirectoryObjectType.Group);

            return await Log(t => t.DirectoryEntryAuditLogs,
            AuditActions.Group_Deleted, deletedEntry);
        }
        public override async Task<bool> Searched(IDirectoryEntryAdapter searchedEntry) => await Log(c => c.DirectoryEntryAuditLogs, AuditActions.Group_Searched, searchedEntry);

        public async Task<bool> Assigned(IDirectoryEntryAdapter member, IDirectoryEntryAdapter parent)
        {
            Analytics?.ObjectAssigned(ActiveDirectoryObjectType.Group);

            await Log(c => c.DirectoryEntryAuditLogs,
               AuditActions.Group_Assigned,
            member,
               null,
               "Assigned to " + parent.DN);
            await Log(c => c.DirectoryEntryAuditLogs,
              AuditActions.Group_Assigned,
           parent,
              null,
              "Added member " + member.DN);
            return true;
        }
        public async Task<bool> Unassigned(IDirectoryEntryAdapter member, IDirectoryEntryAdapter parent)
        {
            Analytics?.ObjectAssigned(ActiveDirectoryObjectType.Group);

            await Log(c => c.DirectoryEntryAuditLogs,
               AuditActions.Group_Unassigned,
            member,
               null,
               "Unassigned from " + parent.DN);
            await Log(c => c.DirectoryEntryAuditLogs,
              AuditActions.Group_Unassigned,
           parent,
              null,
              "Removed member " + member.DN);
            return true;
        }
        public async Task<bool> MemberAdded(IDirectoryEntryAdapter parent, IDirectoryEntryAdapter member)
        {
            await Log(c => c.DirectoryEntryAuditLogs,
                 AuditActions.Group_Member_Added,
              parent,
                 null,
                 "Added member " + member.DN);
            return true;
        }
        public async Task<bool> MemberRemoved(IDirectoryEntryAdapter parent, IDirectoryEntryAdapter member)
        {
            await Log(c => c.DirectoryEntryAuditLogs,
                 AuditActions.Group_Member_Removed,
              parent,
                 null,
                 "Removed member " + member.DN);
            return true;
        }
        public async Task<bool> Moved(IDirectoryEntryAdapter movedGroup, IADOrganizationalUnit ouMovedFrom, IADOrganizationalUnit ouMovedTo)
        {
            Analytics?.ObjectMoved(ActiveDirectoryObjectType.Group);

            await Log(c => c.DirectoryEntryAuditLogs,
               AuditActions.Group_Moved,
            movedGroup,
               ouMovedFrom.OU,
               ouMovedTo.OU);
            return true;
        }
        public override async Task<bool> Changed(IDirectoryEntryAdapter changedGroup, List<AuditChangeLog> changes)
        {

            await Log(c => c.DirectoryEntryAuditLogs,
                AuditActions.Group_Edited,
                changedGroup,
                changes.GetValueChangesString(c => c.OldValue),
                changes.GetValueChangesString(c => c.NewValue));
            return true;
        }
        public override async Task<bool> Created(IDirectoryEntryAdapter newEntry)
        {
            Analytics?.ObjectCreated(ActiveDirectoryObjectType.Group);

            var oldValues = "";
            var newValues = "";
            foreach (var c in newEntry.NewEntryProperties)
            {
                newValues += c.Key + "=" + c.Value;
            }
            await Log(c => c.DirectoryEntryAuditLogs,
                AuditActions.Group_Created,
                newEntry,
                oldValues,
                newValues);
            return true;
        }
    }
}