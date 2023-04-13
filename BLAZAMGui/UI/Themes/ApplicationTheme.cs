using Microsoft.AspNetCore.Builder.Extensions;
using MudBlazor;
using System.Drawing;
using System.Threading;
using Color = System.Drawing.Color;

namespace BLAZAM.Gui.UI.Themes
{

    public class ApplicationTheme
    {
        public static List<ApplicationTheme> Themes = new List<ApplicationTheme> { new BlueTheme(), new RedTheme() };
        protected Palette pallete { get; set; }
        protected PaletteDark darkPallete { get; set; }
        protected string _name;




        public string Name { get => _name; }

        public MudTheme Theme
        {
            get
            {
                return new MudTheme
                {
                     Palette = pallete,
                      PaletteDark = darkPallete,
                   
                };
            }
        }

        
       

        
    }
}
