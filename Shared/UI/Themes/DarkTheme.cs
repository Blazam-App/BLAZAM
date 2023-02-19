using Microsoft.AspNetCore.Builder.Extensions;
using System.Drawing;

namespace BLAZAM.Server.Shared.UI.Themes
{
    public class DarkTheme : ApplicationTheme
    {
        public DarkTheme()
        {
            _name = "Dark";

            _textLight = Color.Gray.ToHex();
            _textDark = Color.WhiteSmoke.ToHex();
            _light = "#050807";
            _dark = "#202226";
            _primary = "#a9a09f";
            _secondary = "#007389";
            _info = "#1b8f7e";
            _success = "#5fad00";
            _warning = "#ffc270";
            _danger = "#f60066";
            _body = _light;
            _muted = Color.DarkGray.ToHex();
          
        }
    }
}
