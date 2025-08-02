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
        public static readonly string ApplicationIconUri = "/static/img/appicon.png";
        /// <summary>
        /// "/static/img/appicon.png"
        /// </summary>
        public static readonly string MaggiePortraitUri = "/static/img/maggie.png";
        /// <summary>
        /// "/static/img/favicon.ico"
        /// </summary>
        public static readonly string FaviconUri = "/static/img/favicon.ico";

        public static byte[]? Maggie(int size = 1914)
        {
            var defaultIconFilePath = Path.GetFullPath(ApplicationInfo.applicationRoot + MaggiePortraitUri);
            if (File.Exists(defaultIconFilePath))
            {
                var imgBytes = File.ReadAllBytes(defaultIconFilePath);
                if (imgBytes != null)
                {
                    return imgBytes.ResizeRawImage(size);
                }
               

            }
            return null;
        }


        public static byte[]? AppIcon(int size = 250)
        {

            var dbIcon = DatabaseCache.AppIcon;
            if (dbIcon != null)
            {
                return dbIcon.ResizeRawImage(size);
            }
            else
            {
                var defIcon = GetDefaultIcon();
                if (defIcon != null)
                {
                    return defIcon.ResizeRawImage(size);
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
