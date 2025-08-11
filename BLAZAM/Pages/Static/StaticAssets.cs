using BLAZAM.Common.Data;
using BLAZAM.Database.Context;

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
        public static readonly string ApplicationIconUri = Path.DirectorySeparatorChar
                + "static"
                + Path.DirectorySeparatorChar
                + "img"
                + Path.DirectorySeparatorChar
                + "default_logo5.png";
        /// <summary>
        /// "/static/img/appicon.png"
        /// </summary>
        public static readonly string MaggiePortraitUri = Path.DirectorySeparatorChar
                + "static"
                + Path.DirectorySeparatorChar
                + "img"
                + Path.DirectorySeparatorChar
                + "maggie.png";
        /// <summary>
        /// "/static/img/favicon.ico"
        /// </summary>
        public static readonly string FaviconUri = Path.DirectorySeparatorChar
                + "static"
                + Path.DirectorySeparatorChar
                + "img"
                + Path.DirectorySeparatorChar
                + "favicon.ico";

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
            var defaultIconFilePath = Path.GetFullPath(
               ApplicationIconUri
            );
            if (File.Exists(defaultIconFilePath))
                return File.ReadAllBytes(defaultIconFilePath);
            return null;
        }
    }
}
