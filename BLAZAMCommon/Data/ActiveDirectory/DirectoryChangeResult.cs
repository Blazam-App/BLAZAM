using BLAZAM.Common.Data.ActiveDirectory.Interfaces;

namespace BLAZAM.Common.Data.ActiveDirectory
{
    public class DirectoryChangeResult
    {
        public List<IADGroup> AssignedGroups { get; internal set; } = new List<IADGroup>();
        public List<IADGroup> UnassignedGroups { get; internal set; } = new List<IADGroup>();
    }
}