
using BLAZAM.Helpers;
using MudBlazor;


namespace BLAZAM.Themes
{
    public class ApplicationTheme
    {
        public static readonly IReadOnlyCollection<ApplicationTheme> Themes = new List<ApplicationTheme>() { new BlueTheme(), new RedTheme(), new GreenTheme(), new OrangeTheme() };
        protected PaletteLight lightPalette { get; set; }
        protected PaletteDark darkPalette { get; set; }
        protected string _name;

        public ApplicationTheme()
        {
            lightPalette = new()
            {
                TextPrimary = "#050505",
                TextSecondary = System.Drawing.Color.SlateGray.ToHex(),
                HoverOpacity = 0,
                Surface = System.Drawing.Color.WhiteSmoke.ToHex(),
                DarkContrastText = System.Drawing.Color.WhiteSmoke.ToHex(),
                DrawerText = System.Drawing.Color.WhiteSmoke.ToHex(),
                Background = "#efefef",
                Success = "#00c853",
                TextDisabled = System.Drawing.Color.DarkGray.ToHex(),
                White = System.Drawing.Color.White.ToHex()
            };

            darkPalette = new()
            {
                TextPrimary = "#c7c7c7",
                DarkContrastText = "#383b40",
                Surface = "#545960",
                Background = "#383b40",
                DrawerText = "#c7c7c7",
                Success = "#00c853",
                TextDisabled = System.Drawing.Color.DarkGray.ToHex(),
                White = System.Drawing.Color.WhiteSmoke.ToHex(),
                HoverOpacity = 0.25,
            };
        }

        public string Name { get => _name; }

        public MudTheme Theme
        {
            get
            {
                return new MudTheme
                {
                    PaletteLight = lightPalette,
                    PaletteDark = darkPalette,

                };
            }
        }





    }
}
