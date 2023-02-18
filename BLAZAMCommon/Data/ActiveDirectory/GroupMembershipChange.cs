using BLAZAM.Common.Data.ActiveDirectory.Interfaces;

namespace BLAZAM.Common.Data.ActiveDirectory
{
    public class GroupMembershipChange
    {
        public IGroupableDirectoryModel Member { get; }
        public IADGroup Group { get; }

        public GroupMembershipChange(IGroupableDirectoryModel member, IADGroup group)
        {
            Member = member;
            Group = group;
        }
    }
}
