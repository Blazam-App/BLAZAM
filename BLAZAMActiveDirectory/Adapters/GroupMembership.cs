using BLAZAM.ActiveDirectory.Interfaces;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class GroupMembership
    {
        public IADGroup Group { get; set; }
        public IGroupableDirectoryAdapter Member { get; set; }

        public GroupMembership(IADGroup group, IGroupableDirectoryAdapter member)
        {
            Group = group;
            Member = member;
        }
    }
}