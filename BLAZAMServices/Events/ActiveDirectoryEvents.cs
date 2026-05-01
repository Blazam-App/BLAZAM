namespace BLAZAM.Services.Events
{
    public static class ActiveDirectoryEvents
    {


        /// <summary>
        /// Called when a directory entry is changed in some way
        /// </summary>
        public static AppEvent<DirectoryEntryChangedArgs> DirectoryEntryEvent { get; set; } = new();







    }
}
