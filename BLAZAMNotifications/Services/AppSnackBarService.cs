using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BLAZAM.Notifications.Services
{
    public class AppSnackBarService

    {
        private ISnackbar _snackbar { get; set; }

        public AppSnackBarService(ISnackbar snackbar)
        {
            _snackbar = snackbar;
        }

        public void Error(string message, string? title = null)
        {
            _snackbar.Add(message, Severity.Error);

        }
        public void Error(RenderFragment message, string? title = null)
        {
            _snackbar.Add(message, Severity.Error);
        }
        public void Info(string message, string? title = null)
        {
            _snackbar.Add(message, Severity.Info);

        }

        public void Info(RenderFragment message, string? title = null)
        {
            _snackbar.Add(message, Severity.Info);
        }
        public void Warning(string message, string? title = null)
        {
            _snackbar.Add(message, Severity.Warning);

        }

        public void Warning(RenderFragment message, string? title = null)
        {
            _snackbar.Add(message, Severity.Warning);
        }
        public void Success(string message, string? title = null)
        {
            _snackbar.Add(message, Severity.Success);

        }

        public void Success(RenderFragment message, string? title = null)
        {
            _snackbar.Add(message, Severity.Success);
        }
    }
}
