using Blazorise;
using Microsoft.AspNetCore.Builder.Extensions;
using System.Drawing;
using System.Threading;
using Color = System.Drawing.Color;

namespace BLAZAM.Server.Shared.UI.Themes
{

    public class ApplicationTheme
    {
        public static List<ApplicationTheme> Themes = new List<ApplicationTheme>{ new LightTheme(),new DarkTheme() };
        protected ThemeColorOptions _colorOptions { get; set; }
        protected ThemeBackgroundOptions _backgroundOptions { get; set; }
        protected ThemeTextColorOptions _textColorOptions { get; set; }
        protected ThemeBarOptions _barOptions { get; set; }

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
        protected string _danger;
        protected string _link;
        protected string _body;
        protected string _muted;
        protected string _name;
        
        


        public string Name { get => _name; }

        public Theme Theme
        {
            get
            {
                SetThemeColors();
                return new Theme
                {
                    ColorOptions = _colorOptions,
                    BackgroundOptions = _backgroundOptions,
                    TextColorOptions = _textColorOptions,
                    BarOptions = _barOptions
                };
            }
        }

        private void SetThemeColors()
        {
            _colorOptions = new()
            {
                Primary = _primary,
                Secondary = _secondary,
                Success = _success,
                Info = _info,
                Warning = _warning,
                Danger = _danger,
                Light = _light,
                Dark = _dark,
            };

            _backgroundOptions = new()
            {
                Primary = _primary,
                Secondary = _secondary,
                Success = _success,
                Info = _info,
                Warning = _warning,
                Danger = _danger,
                Light = _light,
                Dark = _dark,
                Body = _body,
                Muted = _muted,

            };

            _textColorOptions = new ThemeTextColorOptions()
            {
                Primary = _primary,
                Secondary = _secondary,
                Success = _success,
                Info = _info,
                Warning = _warning,
                Danger = _danger,
                Light = _textLight,
                Dark = _textDark,
                Body = _body,
                Muted = _muted,

            };
            
            _barOptions = new ThemeBarOptions()
            {
                DarkColors = new()
                {
                    BackgroundColor = _dark,
                     DropdownColorOptions = new()
                     {
                          BackgroundColor = _dark
                     },
                      BrandColorOptions = new ThemeBarBrandColorOptions()
                      {
                           BackgroundColor = _dark
                      },
                      /*
                       ItemColorOptions = new()
                       {
                           ActiveBackgroundColor=_primary,
                            HoverBackgroundColor = _secondary,

                       }
*/
                },
            };
            
        }
    }
}
