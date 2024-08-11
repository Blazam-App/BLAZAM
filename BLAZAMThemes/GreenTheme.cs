using BLAZAM.Helpers;
using System.Drawing;

namespace BLAZAM.Themes
{
    public class GreenTheme : ApplicationTheme
    {
        public GreenTheme()
        {



            _name = "Green";


            lightPalette.ActionDefault = "#9AC3A1";

            lightPalette.AppbarBackground = "#1BB836";
            lightPalette.DrawerBackground = "#CFDCD0";
            lightPalette.DrawerText = "#0F1E12";

            lightPalette.Dark = "#002902";
            lightPalette.Primary = "#1CBE38";
            lightPalette.Secondary = "#0A9B22";

            lightPalette.DrawerIcon = "#95B899";


            darkPalette.TextSecondary = "#7EA782";
            darkPalette.ActionDefault = "#7BB18B";

            darkPalette.Dark = "#0F1E12"; 
            darkPalette.Primary = "#66AB72";

            darkPalette.AppbarBackground = "#134018";
            darkPalette.DrawerBackground = "#0F1E12";
            darkPalette.DrawerText = "#c7c7c7"; 
            darkPalette.Secondary = "#51D470";

            darkPalette.DrawerIcon = "#95B899";



        }
    }
}
