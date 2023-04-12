using BLAZAM.Database.Context;
using BLAZAM.Helpers;

namespace BLAZAM.Gui
{
    public class StaticAssets
    {
        public static string ApplicationIconUri = "/static/img/appicon.png";
        public static string FaviconUri = "/static/img/favicon.ico";

        public static byte[] AppIcon(int size = 250)
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
            var defaultIconFilePath = Path.GetFullPath(ApplicationInfo.applicationRoot + @"static\img\default_logo2.png");
            if (File.Exists(defaultIconFilePath))
                return File.ReadAllBytes(defaultIconFilePath);
            return null;
        }
    }
}
