using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BLAZAM.Database.Models.Database
{
    /// <summary>
    /// The base application settings. These are the most general settings, and application wide.
    /// </summary>
    public class AppSettings
    {

        public int AppSettingsId { get; set; }
        /// <summary>
        /// The timestamp of the last update check
        /// </summary>
        /// <remarks>
        /// Not yet implemented.
        /// TODO Implement
        /// </remarks>
        public DateTime? LastUpdateCheck { get; set; }

        /// <summary>
        /// The name given to the application by it's administrator
        /// </summary>
        [Required]
        public string AppName
        {
            get;
            set;
        } = "Blazam";

        /// <summary>
        /// The database flag that indicates the intial setup wizard has been completed
        /// </summary>
        public bool InstallationCompleted { get; set; }

        /// <summary>
        /// An administrator set message for their users
        /// </summary>
        public string? MOTD { get; set; } = "Welcome to Blazam. Head over to the <a href=\"/settings\">settings<a/> page to configure this application.<br/>To remove this message, modify or clear the Homepage Message settings on the <a href=\"/settings\">settings<a/> page.";

        /// <summary>
        /// Administrator setting to indicate whether to forward HTTP requests to HTTPS
        /// </summary>
        public bool ForceHTTPS { get; set; }


        public string? AppFQDN { get; set; }

        /// <summary>
        /// The Google Analytics Id to use for this particular installation
        /// </summary>
        /// <remarks>
        /// This has no effect on the developer Analytics, that is hard-coded.
        /// </remarks>
        public string? AnalyticsId { get; set; }
        public string? UserHelpdeskURL { get; set; }
        public byte[]? AppIcon { get; set; }


        /// <summary>
        /// An abbreviated form of <see cref="AppName"/>.
        /// </summary>
        /// <returns>
        /// <para>If the name has spaces, it will return the first letter of each word.</para>
        /// <para>Otherwise the first 14 letters will be returned</para>
        /// </returns>
        [NotMapped]
        public string AppAbbreviation
        {
            get
            {
                if (AppName != null)
                {
                    if (AppName.Length < 14)
                        return AppName;
                    if (AppName.Contains(" "))
                    {
                        var words = AppName.Split(' ');
                        string abb = "";
                        foreach (var word in words)
                        {
                            abb += word.ToUpper()[0];
                        }
                        return abb;
                    }
                    else
                        return AppName.Substring(0, 14);
                }
                return "";
            }
        }

        /// <summary>
        /// Indicated whether the administrator wants the application to automatically updated
        /// </summary>
        public bool AutoUpdate { get; set; }

        /// <summary>
        /// The time of day the administrator wants the application to perform the <see cref="AutoUpdate"/>, if enabled.
        /// </summary>
        public TimeSpan AutoUpdateTime { get; set; } = TimeSpan.FromHours(2);


        /// <summary>
        /// The release branch to get updates from
        /// </summary>
        [DefaultValue("Stable")]
        public string UpdateBranch { get; set; } = "Stable";
    }
}
