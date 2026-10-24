namespace BLAZAM.ActiveDirectory.Events
{
    public static class ActiveDirectoryEvents
    {


        /// <summary>
        /// Called when a directory entry is changed in some way
        /// </summary>
        public static AppEvent<DirectoryEntryChangedEventArgs> DirectoryEntryEvent { get; set; } = new();







    }
}
