using BLAZAM.Helpers;
using System.Drawing;

namespace BLAZAM.Themes
{
    public class BlueTheme : ApplicationTheme
    {
        public BlueTheme()
        {



            _name = "Blue";


            lightPalette.ActionDefault = "#9aafc3";

            lightPalette.AppbarBackground = "#2261d3";
            lightPalette.DrawerBackground = "#cfd6dc";
            lightPalette.DrawerText = "#0f141e";
            lightPalette.Dark = "#001529";
            lightPalette.Primary = "#2261d3";
            lightPalette.Secondary = "#40449D";



            darkPalette.TextSecondary = "#7e95a7";
            darkPalette.ActionDefault = "#7b9ab1";
            darkPalette.Dark = "#0f141e";
            darkPalette.Primary = "#132a40";
            darkPalette.AppbarBackground = "#132a40";
            darkPalette.DrawerBackground = "#0f141e";
            darkPalette.DrawerText = "#c7c7c7";
            darkPalette.Secondary = "#407DAB";



        }
    }
}
