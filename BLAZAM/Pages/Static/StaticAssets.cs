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
        public static readonly string ApplicationIconUri = "/static/img/appicon.png";
        /// <summary>
        /// "/static/img/favicon.ico"
        /// </summary>
        public static readonly string FaviconUri = "/static/img/favicon.ico";




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
            var defaultIconFilePath = Path.GetFullPath(ApplicationInfo.applicationRoot.FullPath +
                Path.DirectorySeparatorChar +
                "wwwroot" +
                Path.DirectorySeparatorChar +
                "img" +
                Path.DirectorySeparatorChar +
                "default_logo.png");
            if (File.Exists(defaultIconFilePath))
                return File.ReadAllBytes(defaultIconFilePath);
            return null;
        }
    }
}
