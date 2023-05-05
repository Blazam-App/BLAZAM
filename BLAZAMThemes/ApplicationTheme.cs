
using MudBlazor;


namespace BLAZAM.Themes
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
