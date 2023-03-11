using BLAZAM.Common.Data.ActiveDirectory.Interfaces;

namespace BLAZAM.Common.Data.ActiveDirectory
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
