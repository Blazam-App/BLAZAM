using BLAZAM.Common.Extensions;
using Microsoft.AspNetCore.Builder.Extensions;
using System.Drawing;

namespace BLAZAM.Server.Shared.UI.Themes
{
    public class LightTheme : ApplicationTheme
    {
        public LightTheme()
        {

            _name = "Light";


            _textPrimary = "#050505";
            _textSecondary = Color.SlateGray.ToHex();

            _light = Color.WhiteSmoke.ToHex();
            _dark = "#001529";
            _primary = "#2261d3";
            _secondary = "#0c13a7";
            _info = "#46a9ef";
            _success = Color.ForestGreen.ToHex();
            _warning = Color.Gold.ToHex();
            error = Color.Red.ToHex();
            _body = Color.LightGray.ToHex();
            _muted = Color.DarkGray.ToHex();
            _white = Color.White.ToHex();
        }
    }
}
