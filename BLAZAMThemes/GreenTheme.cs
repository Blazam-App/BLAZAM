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

            lightPalette.AppbarBackground = "#22D340";
            lightPalette.DrawerBackground = "#00290F"; ;

            lightPalette.Dark = "#002902";
            lightPalette.Primary = "#22D340";
            lightPalette.Secondary = "#0CA726";



            darkPalette.TextSecondary = "#7EA782";
            darkPalette.ActionDefault = "#7BB18B";

            darkPalette.Dark = "#0F1E12";
            darkPalette.Primary = "#13401B";
            darkPalette.AppbarBackground = "#134018";
            darkPalette.DrawerBackground = "#0F1E12";
            darkPalette.DrawerText = "#c7c7c7";
            darkPalette.Secondary = "#4CAE69";



        }
    }
}
