using BLAZAM.Common.Extensions;
using Microsoft.AspNetCore.Builder.Extensions;
using System.Drawing;

namespace BLAZAM.Server.Shared.UI.Themes
{
    public class DarkTheme : ApplicationTheme
    {
        public DarkTheme()
        {
            _name = "Dark";

            _textPrimary = "#c9c6c6";
            _textSecondary = "#c5cbd3";

            _light = "#383b40";
            _dark = "#202226";
            _primary = "#a9a09f";
            _secondary = "#183042";
            _info = "#1b8f7e";
            _success = "#5fad00";
            _warning = "#ffc270";
            error = "#f60066";
            _body = _light;
            _muted = Color.DarkGray.ToHex();
            _white= Color.WhiteSmoke.ToHex();

        }
    }
}
