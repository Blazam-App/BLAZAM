﻿using BLAZAM.Database.Context;

namespace BLAZAM.Server.Pages
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
            var defaultIconFilePath = Path.GetFullPath(Program.RootDirectory + @"static\img\default_logo2.png");
            if (System.IO.File.Exists(defaultIconFilePath))
                return System.IO.File.ReadAllBytes(defaultIconFilePath);
            return null;
        }
    }
}
