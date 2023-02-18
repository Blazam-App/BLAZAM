using BLAZAM.Common.Data.ActiveDirectory.Interfaces;

namespace BLAZAM.Common.Data.ActiveDirectory.Models
{
    public class GroupMembership
    {
        public IADGroup Group { get; set; }
        public IGroupableDirectoryModel Member { get; set; }

        public GroupMembership(IADGroup group, IGroupableDirectoryModel member)
        {
            Group = group;
            Member = member;
        }
    }
}