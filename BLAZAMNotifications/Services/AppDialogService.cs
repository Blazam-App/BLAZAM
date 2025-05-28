using BLAZAM.Helpers;
using BLAZAM.Logger; // Added
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System; // Added
using System.Threading.Tasks; // Added for Task

namespace BLAZAM.Notifications.Services
{
    /// <summary>
    /// Provides a wrapper around MudBlazor's <see cref="IDialogService"/> for showing application-styled dialogs and message boxes, with added logging capabilities.
    /// </summary>
    public class AppDialogService
    {
        private IDialogService _dialog { get; set; }
        private DialogOptions DialogOptions { get; set; } = new DialogOptions() { };

        /// <summary>Shows a dialog with a custom component content.</summary>
        /// <typeparam name="TComponent">The type of the component to render inside the dialog.</typeparam>
        /// <param name="parameters">Parameters to pass to the component.</param>
        /// <param name="title">Optional title for the dialog.</param>
        /// <param name="yesText">Optional text for the 'Yes' button (if applicable within TComponent or options).</param>
        /// <param name="noText">Optional text for the 'No' button (if applicable within TComponent or options).</param>
        /// <param name="cancelText">Optional text for the 'Cancel' button (if applicable within TComponent or options).</param>
        /// <param name="options">Optional dialog options.</param>
        /// <returns>A reference to the dialog instance.</returns>
        public async Task<IDialogReference> ShowMessage<TComponent>(DialogParameters parameters, string? title = null, string? yesText = null, string? noText = null, string? cancelText = null, DialogOptions? options = null) where TComponent : ComponentBase, new()
        {
            Loggers.SystemLogger.Debug("AppDialogService.ShowMessage<TComponent>: Attempting to show dialog with Title '{DialogTitle}' using component {ComponentName}.", title, typeof(TComponent).Name);
            return await _dialog.ShowAsync<TComponent>(title, parameters, options ?? DialogOptions);
        }

        /// <summary>Shows a pre-formatted message box.</summary>
        private async Task<bool?> ShowMessage(MarkupString message, string? title = null, string? yesText = null, string? noText = null, string? cancelText = null)
        {
            Loggers.SystemLogger.Debug("AppDialogService.ShowMessage (MarkupString): Attempting to show message box with Title '{DialogTitle}'.", title);
            return await _dialog.ShowMessageBox(title, message, yesText, noText, cancelText, DialogOptions);
        }
        
        /// <summary>Shows a pre-formatted message box.</summary>
        private async Task<bool?> ShowMessage(string message, string? title = null)
        {
            Loggers.SystemLogger.Debug("AppDialogService.ShowMessage (string): Attempting to show message box with Title '{DialogTitle}'.", title);
            return await ShowMessage(message.ToMarkupString(), title);
        }

        /// <summary>Initializes a new instance of the <see cref="AppDialogService"/> class.</summary> 
        /// <param name="dialog">The MudBlazor <see cref="IDialogService"/> to be wrapped.</param> 
        /// <exception cref="ArgumentNullException">Thrown if dialog service is null.</exception>
        public AppDialogService(IDialogService dialog)
        {
            ArgumentNullException.ThrowIfNull(dialog);

            _dialog = dialog;
        }

        /// <summary>Displays an error message box.</summary> 
        /// <param name="message">The message to display.</param> 
        /// <param name="title">Optional title for the message box.</param> 
        /// <returns>A task that represents the asynchronous dialog operation, returning a boolean indicating if 'Yes' (or equivalent) was pressed, or null if cancelled/closed.</returns>
        public async Task<bool?> Error(string message, string? title = null)
        {
            Loggers.SystemLogger.Debug("AppDialogService.Error: Displaying error dialog with Title '{DialogTitle}'.", title);
            return await ShowMessage(message, title);
        }

        /// <summary>Displays an info message box.</summary> 
        /// <param name="message">The message to display.</param> 
        /// <param name="title">Optional title for the message box.</param> 
        /// <returns>A task that represents the asynchronous dialog operation, returning a boolean indicating if 'Yes' (or equivalent) was pressed, or null if cancelled/closed.</returns>
        public async Task<bool?> Info(string message, string? title = null)
        {
            Loggers.SystemLogger.Debug("AppDialogService.Info: Displaying info dialog with Title '{DialogTitle}'.", title);
            return await ShowMessage(message, title);
        }

        /// <summary>Displays a warning message box.</summary> 
        /// <param name="message">The message to display.</param> 
        /// <param name="title">Optional title for the message box.</param> 
        /// <returns>A task that represents the asynchronous dialog operation, returning a boolean indicating if 'Yes' (or equivalent) was pressed, or null if cancelled/closed.</returns>
        public async Task<bool?> Warning(string message, string? title = null)
        {
            Loggers.SystemLogger.Debug("AppDialogService.Warning: Displaying warning dialog with Title '{DialogTitle}'.", title);
            return await ShowMessage(message, title);
        }

        /// <summary>Displays a success message box.</summary> 
        /// <param name="message">The message to display.</param> 
        /// <param name="title">Optional title for the message box.</param> 
        /// <returns>A task that represents the asynchronous dialog operation, returning a boolean indicating if 'Yes' (or equivalent) was pressed, or null if cancelled/closed.</returns>
        public async Task<bool?> Success(string message, string? title = null)
        {
            Loggers.SystemLogger.Debug("AppDialogService.Success: Displaying success dialog with Title '{DialogTitle}'.", title);
            return await ShowMessage(message, title);
        }

        /// <summary>Displays a confirmation message box with OK and Cancel buttons.</summary> 
        /// <param name="message">The confirmation message.</param> 
        /// <param name="title">Optional title for the message box.</param> 
        /// <returns>A task that represents the asynchronous dialog operation, returning true if OK was pressed, false otherwise.</returns>
        public async Task<bool> Confirm(string message, string? title = null)
        {
            Loggers.SystemLogger.Debug("AppDialogService.Confirm: Displaying confirmation dialog with Title '{DialogTitle}'.", title);
            return await _dialog.ShowMessageBox(title, message, "OK", null, "Cancel") == true;
        }
    }
}
