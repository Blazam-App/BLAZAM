using BLAZAM.ActiveDirectory.Interfaces;

namespace BLAZAM.ActiveDirectory
{
    /// <summary>
    /// Used to store membership changes for auditing purposes
    /// </summary>
    public class GroupMembershipChange
    {
        public IGroupableDirectoryAdapter Member { get; }
        public IADGroup Group { get; }

        public GroupMembershipChange(IGroupableDirectoryAdapter member, IADGroup group)
        {
            Member = member;
            Group = group;
        }
    }
}
