namespace BLAZAM.Services.Events
{
    public static class ApplicationEvents
    {
        /// <summary>
        /// Called when permission are changed by an admin
        /// </summary>
        public static AppEvent PermissionsChanged { get; set; } = new();



        /// <summary>
        /// Called when a template is added or removed
        /// </summary>
        public static AppEvent TemplatesChanged { get; set; } = new();


        /// <summary>
        /// Called when a directory entry is changed in some way
        /// </summary>
        public static AppEvent<DirectoryEntryChangedArgs> DirectoryEntryEvent { get; set; } = new();





    }
}
