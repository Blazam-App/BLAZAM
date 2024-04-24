using BLAZAM.Helpers;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BLAZAM.Notifications.Services
{
    public class AppDialogService
    {
        private IDialogService _dialog { get; set; }
        DialogOptions DialogOptions { get; set; } = new DialogOptions() { };

        public async Task<IDialogReference> ShowMessage<TComponent>(DialogParameters parameters , string? title = null, string? yesText = null, string? noText = null, string? cancelText = null, DialogOptions? options=null) where TComponent : ComponentBase, new()
        {
           return await _dialog.ShowAsync<TComponent>(title, parameters, options??DialogOptions);
        }


        private async Task<bool?> ShowMessage(MarkupString message, string? title = null, string? yesText = null, string? noText = null, string? cancelText = null)
        {
            return await _dialog.ShowMessageBox(title, message, yesText, noText, cancelText,DialogOptions);
        }

        private async Task<bool?> ShowMessage(string message, string? title = null)
        {
           return await ShowMessage(message.ToMarkupString(), title);
        }


        public AppDialogService(IDialogService dialog)
        {
            _dialog = dialog;
        }

        

        public async Task<bool?> Error(string message, string? title = null)
        {
            
            return await ShowMessage(message, title);
        }


        public async Task<bool?> Info(string message, string? title = null)

        {
            return await ShowMessage(message, title);

        }
        public async Task<bool?> Warning(string message, string? title = null)

        {
            return await ShowMessage(message, title);

        }
        public async Task<bool?> Success(string message, string? title = null)

        {

            return await ShowMessage(message, title);


        }
        public async Task<bool> Confirm(string message, string? title = null)

        {
            return await _dialog.ShowMessageBox(title, message, "OK", null, "Cancel") == true;


        }


    }
}
