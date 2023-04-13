using System.DirectoryServices;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    public interface IADGroup : IGroupableDirectoryAdapter, IComparable
    {
        string GroupName { get; set; }
        bool HasMembers { get; }
        List<IADUser> UserMembers { get; }
        List<IADGroup> GroupMembers { get; }
        List<IGroupableDirectoryAdapter> Members { get; }
        List<string> MembersAsStrings { get; }
        IEnumerable<IGroupableDirectoryAdapter> NestedMembers { get; }

        void AssignMember(IGroupableDirectoryAdapter member);
        void UnassignMember(IGroupableDirectoryAdapter member);
    }
}