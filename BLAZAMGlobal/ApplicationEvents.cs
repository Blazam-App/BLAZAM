using BLAZAM.Global.Events;

namespace BLAZAM.Global
{
    public static class ApplicationEvents
    {

        /// <summary>
        /// Called when a user logs in or out of the application
        /// </summary>
        public static AppEvent<int> LoggedOnUserCountChanged { get; set; } = new();

        /// <summary>
        /// Called when permission are changed by an admin
        /// </summary>
        public static AppEvent PermissionsChanged { get; set; } = new();

    }
}
