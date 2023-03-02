using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BLAZAM.Common.Data.Database;

namespace BLAZAM.Common.Models.Database
{
    public class AppSettings
    {

        public int AppSettingsId { get; set; }
        public DateTime? LastUpdateCheck { get; set; }
        [Required]
        public string AppName
        {
            get;
            set;
        } = "Blazam";
        public bool InstallationCompleted { get; set; }
        public string? MOTD { get; set; } = "Welcome to Blazam. Head over to the <a href=\"/settings\">settings<a/> page to configure this application.<br/>To remove this message, modify or clear the Homepage Message settings on the <a href=\"/settings\">settings<a/> page.";
        public bool ForceHTTPS { get; set; }
        public int? HttpsPort { get; set; }
        public string? AppFQDN { get; set; }
        /// <summary>
        /// The Google Analytics Id to use
        /// </summary>
        /// <remarks>This has no effect on the developer Analytics, that is hard-coded.</remarks>
        public string? AnalyticsId { get; set; }
        public string? UserHelpdeskURL { get; set; }
        public byte[]? AppIcon { get; set; }
        [NotMapped]
        public string AppAbbreviation
        {
            get
            {
                if (AppName != null)
                {
                    var words = AppName.Split(' ');
                    string abb = "";
                    foreach (var word in words)
                    {
                        abb += word.ToUpper()[0];
                    }
                    return abb;
                }
                return "";
            }
        }

        public bool AutoUpdate { get; set; }
        public TimeSpan AutoUpdateTime { get; set; } = TimeSpan.FromHours(2);

        [DefaultValue("Stable")]
        public string UpdateBranch { get; set; } = "Stable";
    }
}
