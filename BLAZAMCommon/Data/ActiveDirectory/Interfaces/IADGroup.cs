using System.DirectoryServices;

namespace BLAZAM.Common.Data.ActiveDirectory.Interfaces
{
    public interface IADGroup : IGroupableDirectoryModel, IComparable
    {
        string GroupName { get; set; }
        bool HasMembers { get; }
        List<IADUser> UserMembers { get; }
        List<IADGroup> GroupMembers { get; }

        void AssignMember(IGroupableDirectoryModel member);
        void UnassignMember(IGroupableDirectoryModel member);
    }
}