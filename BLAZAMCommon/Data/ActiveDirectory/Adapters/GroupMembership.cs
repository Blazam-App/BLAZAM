using BLAZAM.Common.Data.ActiveDirectory.Interfaces;

namespace BLAZAM.Common.Data.ActiveDirectory.Models
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