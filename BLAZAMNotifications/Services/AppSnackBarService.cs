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

        public void Error(string message)
        {
            _snackbar.Add(message, Severity.Error);

        }
        public void Error(RenderFragment message)
        {
            _snackbar.Add(message, Severity.Error);
        }
        public void Info(string message)
        {
            _snackbar.Add(message, Severity.Info);

        }

        public void Info(RenderFragment message)
        {
            _snackbar.Add(message, Severity.Info);
        }
        public void Warning(string message)
        {
            _snackbar.Add(message, Severity.Warning);

        }

        public void Warning(RenderFragment message)
        {
            _snackbar.Add(message, Severity.Warning);
        }
        public void Success(string message)
        {
            _snackbar.Add(message, Severity.Success);

        }

        public void Success(RenderFragment message)
        {
            _snackbar.Add(message, Severity.Success);
        }
    }
}
