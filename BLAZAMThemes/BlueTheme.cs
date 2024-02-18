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
            lightPalette.DrawerBackground = "#001529";

            lightPalette.Dark = "#001529";
            lightPalette.Primary = "#2261d3";
            lightPalette.Secondary = "#0c13a7";



            darkPalette.TextSecondary = "#7e95a7";
            darkPalette.ActionDefault = "#7b9ab1";
            darkPalette.Dark = "#0f141e";
            darkPalette.Primary = "#132a40";
            darkPalette.AppbarBackground = "#132a40";
            darkPalette.DrawerBackground = "#0f141e";
            darkPalette.DrawerText = "#c7c7c7";
            darkPalette.Secondary = "#2b5676";



        }
    }
}
