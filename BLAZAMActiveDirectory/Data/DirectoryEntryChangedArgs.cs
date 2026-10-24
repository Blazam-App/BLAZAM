using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;

namespace BLAZAM.ActiveDirectory.Events
{
    public class DirectoryEntryChangedArgs : BaseEventArgs
    {
        public IDirectoryEntryAdapter Entry { get; set; }

        public List<AuditChangeLog> Changes { get; set; }

        public IDirectoryEntryAdapter? Target { get; set; }

        public IDirectoryEntryAdapter? Origin { get; set; }

        public IDirectoryEntryAdapter? OriginalEntry { get; set; }

        public ActiveDirectoryObjectType ObjectType { get => Entry.ObjectType; }


    }
}
