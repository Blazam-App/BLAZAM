using BLAZAM.ActiveDirectory.Interfaces;

namespace BLAZAM.ActiveDirectory
{
    [Obsolete("Never implemented")]
    public class DirectoryChangeResult
    {
        public List<IADGroup> AssignedGroups { get; internal set; } = new List<IADGroup>();
        public List<IADGroup> UnassignedGroups { get; internal set; } = new List<IADGroup>();
        public List<IGroupableDirectoryAdapter> AssignedMembers { get; internal set; } = new List<IGroupableDirectoryAdapter>();
        public List<IGroupableDirectoryAdapter> UnassignedMembers { get; internal set; } = new List<IGroupableDirectoryAdapter>();
    }
}