namespace BLAZAM.Global.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoStartBackgroundService : Attribute
    {
        public bool Immediate { get; private set; }
        public bool RunOnLinux { get; private set; }

        /// <summary>
        /// Indicates that this service should automatically start 
        /// before a user loads a web page
        /// </summary>
        /// <param name="immediate">If set the servic starts with the application,
        /// otherwise, it will start after a random delay of 15-45 seconds.</param>
        public AutoStartBackgroundService(bool immediate = false, bool runOnLinux = true)
        {
            Immediate = immediate;
            RunOnLinux = runOnLinux;
        }
    }
}
