using BLAZAM.ActiveDirectory.Interfaces;

namespace BLAZAM.ActiveDirectory
{
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
