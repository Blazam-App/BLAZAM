using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BLAZAM.Server.Data.Services
{
    public class AppDialogService
    {
        private IDialogService _dialog { get; set; }

        public AppDialogService(IDialogService dialog)
        {
            _dialog = dialog;
        }

        public async Task Error(string message, string? title = null)
        {




            await _dialog.ShowMessageBox(title, message);
        }
        public async Task Info(string message, string? title = null)

        {
            await _dialog.ShowMessageBox(title, message);

        }
        public async Task Warning(string message, string? title = null)

        {
            await _dialog.ShowMessageBox(title, message);

        }
        public async Task Success(string message, string? title = null)

        {
            await _dialog.ShowMessageBox(title, message);


        }
        public async Task<bool> Confirm(string message, string? title = null)

        {
            return await _dialog.ShowMessageBox(title, message)==true;


        }
    }
}
