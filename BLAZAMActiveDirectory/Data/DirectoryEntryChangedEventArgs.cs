namespace BLAZAM.ActiveDirectory.Events
{
    public class DirectoryEntryChangedEventArgs : BaseAuditEventArgs
    {
        public IDirectoryEntryAdapter Entry { get; set; }

        public List<AuditChangeLog> Changes { get; set; }

        public IDirectoryEntryAdapter? Target { get; set; }

        public IDirectoryEntryAdapter? Origin { get; set; }

        public IDirectoryEntryAdapter? OriginalEntry { get; set; }

        public ActiveDirectoryObjectType ObjectType { get => Entry.ObjectType; }


    }
}
