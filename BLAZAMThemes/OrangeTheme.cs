using BLAZAM.Helpers;
using System.Drawing;

namespace BLAZAM.Themes
{
    public class OrangeTheme : ApplicationTheme
    {
        public OrangeTheme()
        {



            _name = "Orange";


            lightPalette.ActionDefault = "#C3AF9A";

            lightPalette.AppbarBackground = "#D39E22";
            lightPalette.DrawerBackground = "#291F00"; ;

            lightPalette.Dark = "#291D00";
            lightPalette.Primary = "#D39322";
            lightPalette.Secondary = "#A76F0C";



            darkPalette.TextSecondary = "#A7967E";
            darkPalette.ActionDefault = "#B19A7B";

            darkPalette.Dark = "#1E180F";
            darkPalette.Primary = "#402D13";
            darkPalette.AppbarBackground = "#403313";
            darkPalette.DrawerBackground = "#1E1A0F";
            darkPalette.DrawerText = "#c7c7c7";
            darkPalette.Secondary = "#765B2B";



        }
    }
}
