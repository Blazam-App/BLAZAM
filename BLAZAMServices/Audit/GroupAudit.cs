using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Helpers;
using BLAZAM.Database.Context;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Services.Audit
{
    public class GroupAudit : DirectoryAudit
    {
        public GroupAudit(IAppDatabaseFactory factory,
            IApplicationUserStateService userStateService) : base(factory, userStateService)
        {
        }
        public override async Task<bool> Deleted(IDirectoryEntryAdapter deletedEntry)
         => await Log(t => t.DirectoryEntryAuditLogs,
             AuditActions.Group_Deleted, deletedEntry);
        public override async Task<bool> Searched(IDirectoryEntryAdapter searchedGroup) => await Log(c => c.DirectoryEntryAuditLogs, AuditActions.Group_Searched, searchedGroup);

        public async Task<bool> Assigned(IDirectoryEntryAdapter member, IDirectoryEntryAdapter parent)
        {
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
        public async Task<bool> MemberAdded(IDirectoryEntryAdapter parent,IDirectoryEntryAdapter member)
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
                 AuditActions.Group_Member_Added,
              parent,
                 null,
                 "Removed member " + member.DN);
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
        public override async Task<bool> Created(IDirectoryEntryAdapter newGroup)
        {
            var oldValues = "";
            var newValues = "";
            foreach (var c in newGroup.NewEntryProperties)
            {
                newValues += c.Key + "=" + c.Value;
            }
            await Log(c => c.DirectoryEntryAuditLogs,
                AuditActions.Group_Created,
                newGroup,
                oldValues,
                newValues);
            return true;
        }
    }
}