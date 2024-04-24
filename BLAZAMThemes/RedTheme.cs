using BLAZAM.Helpers;
using System.Drawing;


namespace BLAZAM.Themes
{
    public class RedTheme : ApplicationTheme
    {
        public RedTheme()
        {
            _name = "Red";


            lightPalette.ActionDefault = "#C39A9A";
            lightPalette.AppbarBackground = "#D32222";
            lightPalette.DrawerBackground = "#DCCFCF";
            lightPalette.DrawerText = "#1E0F0F";
            lightPalette.Dark = "#290500";
            lightPalette.Primary = "#D32222";
            lightPalette.Secondary = "#A70C0C";


            darkPalette.TextSecondary = "#A77E86";
            darkPalette.ActionDefault = "#B17B7E";
            darkPalette.Dark = "#1E110F";
            darkPalette.Primary = "#AB6666";

            darkPalette.Primary = "#401313";
            darkPalette.AppbarBackground = "#401313";
            darkPalette.DrawerBackground = "#1E0F0F";
            darkPalette.Secondary = "#D45151";



        }
    }
}
