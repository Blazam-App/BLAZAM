using Microsoft.AspNetCore.Builder.Extensions;
using MudBlazor;
using System.Drawing;
using System.Threading;
using Color = System.Drawing.Color;

namespace BLAZAM.Server.Shared.UI.Themes
{

    public class ApplicationTheme
    {
        public static List<ApplicationTheme> Themes = new List<ApplicationTheme> { new LightTheme(), new DarkTheme() };
        protected Palette pallete { get; set; }
        protected PaletteDark darkPallete { get; set; }
      

        protected string _textWhite = "#ffffff";
        protected string _textPrimary;
        protected string _textSecondary;
        protected string _textLight;
        protected string _textDark;
        protected string _light;
        protected string _white;
        protected string _dark;
        protected string _primary;
        protected string _secondary;
        protected string _info;
        protected string _success;
        protected string _warning;
        protected string error;
        protected string _link;
        protected string _body;
        protected string _muted;
        protected string _name;




        public string Name { get => _name; }

        public MudTheme Theme
        {
            get
            {
                SetThemeColors();
                return new MudTheme
                {
                     Palette = pallete,
                      PaletteDark = darkPallete,
                   
                };
            }
        }

        private void SetThemeColors()
        {
            pallete = new()
            {
                Primary = _primary,
                Secondary = _secondary,
                Success = _success,
                Info = _info,
                Warning = _warning,
                Error = error,
                TextDisabled= _muted,
                 TextPrimary= _textPrimary,
                 TextSecondary = _textSecondary,
                  White = _white,
                //Light = _light,
                Dark = _dark,
            };

       

        }
    }
}
