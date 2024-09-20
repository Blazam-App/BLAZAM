using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using BLAZAM.Helpers;

namespace BLAZAM.Static
{
    /// <summary>
    /// A store of static uri's for things like icons and images
    /// </summary>
    public class StaticAssets
    {
        /// <summary>
        /// "/static/img/appicon.png"
        /// </summary>
        public static string ApplicationIconUri = "/static/img/appicon.png";
        /// <summary>
        /// "/static/img/favicon.ico"
        /// </summary>
        public static string FaviconUri = "/static/img/favicon.ico";

        public static byte[]? AppIcon(int size = 250)
        {

            var dbIcon = DatabaseCache.AppIcon;
            if (dbIcon != null)
            {
                return dbIcon.ReizeRawImage(size);
            }
            else
            {
                var defIcon = GetDefaultIcon();
                if (defIcon != null)
                {
                    return defIcon.ReizeRawImage(size);
                }
            }
            return null;
        }


        private static byte[]? GetDefaultIcon()
        {
            var defaultIconFilePath = Path.GetFullPath(ApplicationInfo.applicationRoot + @"\static\img\default_logo5.png");
            if (File.Exists(defaultIconFilePath))
                return File.ReadAllBytes(defaultIconFilePath);
            return null;
        }
    }
}
