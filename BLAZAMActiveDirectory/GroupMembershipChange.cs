using BLAZAM.ActiveDirectory.Interfaces;

namespace BLAZAM.ActiveDirectory
{

    public enum GroupMembershipAction { Assign, Unassign }
    /// <summary>
    /// Used to store membership changes for auditing purposes
    /// </summary>
    public class GroupMembershipChange
    {
        public IGroupableDirectoryAdapter Member { get; }
        public IADGroup Group { get; }
        public GroupMembershipAction Action { get; }

        public GroupMembershipChange(IGroupableDirectoryAdapter member, IADGroup group, GroupMembershipAction action)
        {
            Member = member;
            Group = group;
            Action = action;
        }
    }
}
