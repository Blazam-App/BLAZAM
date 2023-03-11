using System.DirectoryServices;

namespace BLAZAM.Common.Data.ActiveDirectory.Interfaces
{
    public interface IADGroup : IGroupableDirectoryAdapter, IComparable
    {
        string GroupName { get; set; }
        bool HasMembers { get; }
        List<IADUser> UserMembers { get; }
        List<IADGroup> GroupMembers { get; }
        //List<IGroupableDirectoryModel> Members { get;}
        void AssignMember(IGroupableDirectoryAdapter member);
        void UnassignMember(IGroupableDirectoryAdapter member);
    }
}