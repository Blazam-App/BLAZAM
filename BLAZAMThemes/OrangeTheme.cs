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
            lightPalette.DrawerBackground = "#DCD8CF";
            lightPalette.DrawerText = "#1E1A0F";

            lightPalette.Dark = "#291D00";
            lightPalette.Primary = "#D39322";
            lightPalette.Secondary = "#A76F0C";

            lightPalette.DrawerIcon = "#B8AB95";



            darkPalette.DarkContrastText = "#c7c7c7";
            darkPalette.TextPrimary = "#c7c7c7";
            darkPalette.TextSecondary = "#A7967E";
            darkPalette.ActionDefault = "#B19A7B";

            darkPalette.Dark = "#1E180F";

            darkPalette.Primary = "#AB8966";
            darkPalette.AppbarBackground = "#403313";
            darkPalette.DrawerBackground = "#1E1A0F";
            darkPalette.DrawerText = "#c7c7c7";

            darkPalette.Secondary = "#D49C51";

            darkPalette.DrawerIcon = "#B8AB95";


        }
    }
}
