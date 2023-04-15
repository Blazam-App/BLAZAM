using BLAZAM.Helpers;
using System.Drawing;

namespace BLAZAM.Themes
{
    public class BlueTheme : ApplicationTheme
    {
        public BlueTheme()
        {



            _name = "Blue";


            pallete = new()
            {
                TextPrimary = "#050505",
                TextSecondary = Color.SlateGray.ToHex(),
                ActionDefault = "#9aafc3",
                HoverOpacity = 0,
                Surface = Color.WhiteSmoke.ToHex(),
                DarkContrastText = Color.WhiteSmoke.ToHex(),
                AppbarBackground = "#2261d3",
                DrawerBackground = "#001529",
                DrawerText = Color.WhiteSmoke.ToHex(),
                Background = "#efefef",
                //_textDark = "#001529",
                Dark = "#001529",
                Primary = "#2261d3",
                Secondary = "#0c13a7",
                Info = "#46a9ef",
                Success = Color.ForestGreen.ToHex(),
                Warning = "#ff9900",
                Error = Color.Red.ToHex(),
                //_body = Color.LightGray.ToHex(),
                TextDisabled = Color.DarkGray.ToHex(),
                White = Color.White.ToHex(),
            };

            darkPallete = new()
            {
                TextPrimary = "#c7c7c7",
                TextSecondary = "#7e95a7",
                ActionDefault = "#7b9ab1",
                DarkContrastText = "#383b40",
                Surface = "#545960",
                Background = "#383b40",
                // _textDark = "#202226",
                Dark = "#0f141e",
                Primary = "#132a40",
                AppbarBackground = "#132a40",
                DrawerBackground = "#0f141e",
                DrawerText = "#c7c7c7",
                Secondary = "#183042",
                Info = "#1b8f7e",
                Success = "#5fad00",
                Warning = "#ffc270",
                Error = "#f60066",
                //_body = _light,
                TextDisabled = Color.DarkGray.ToHex(),
                White = Color.WhiteSmoke.ToHex(),
                HoverOpacity = 0.25,
            };

        }
    }
}
