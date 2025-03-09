namespace BLAZAM.Common.Attributes
{
    public class AutoStartBackgroundService : Attribute
    {
        public bool Immediate { get; set; }

        /// <summary>
        /// Indicates that this service should automatically start 
        /// before a user loads a web page
        /// </summary>
        /// <param name="immediate">If set the servic starts with the application,
        /// otherwise, it will start after a random delay of 15-45 seconds.</param>
        public AutoStartBackgroundService(bool immediate = false)
        {
            Immediate = immediate;
        }
    }
}
