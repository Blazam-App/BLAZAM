using Microsoft.AspNetCore.Builder.Extensions;
using System.Drawing;

namespace BLAZAM.Server.Shared.UI.Themes
{
    public class RedTheme : ApplicationTheme
    {
        public RedTheme()
        {
            _name = "Red";


            pallete = new()
            {
                TextPrimary = "#050505",
                TextSecondary = Color.SlateGray.ToHex(),
                ActionDefault = "#C39A9A",
                HoverOpacity = 0,
                Surface = Color.WhiteSmoke.ToHex(),
                DarkContrastText = Color.WhiteSmoke.ToHex(),
                AppbarBackground = "#D32222",
                DrawerBackground = "#290300",
                DrawerText = Color.WhiteSmoke.ToHex(),
                Background = "#efefef",
                Dark = "#001529",
                Primary = "#D32222",
                Secondary = "#A7A00C",
                Info = "#46A9EF",
                Success = Color.ForestGreen.ToHex(),
                Warning = Color.Gold.ToHex(),
                Error = Color.Red.ToHex(),
                TextDisabled = Color.DarkGray.ToHex(),
                White = Color.White.ToHex(),
            };

            darkPallete = new()
            {
                TextPrimary = "#c7c7c7",
                TextSecondary = "#A77E86",
                ActionDefault = "#B17B7E",
                DarkContrastText = "#383b40",
                Surface = "#545960",
                Background = "#383b40",
                Dark = "#0f141e",
                Primary = "#401313",
                AppbarBackground = "#401313",
                DrawerBackground = "#1E0F0F",
                DrawerText = "#c7c7c7",
                Secondary = "#421818",
                Info = "#1b8f7e",
                Success = "#5fad00",
                Warning = "#ffc270",
                Error = "#f60066",
                TextDisabled = Color.DarkGray.ToHex(),
                White = Color.WhiteSmoke.ToHex(),
                HoverOpacity = 0.25,
            };

        }
    }
}
