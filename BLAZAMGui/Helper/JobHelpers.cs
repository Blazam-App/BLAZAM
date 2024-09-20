using BLAZAM.Gui.UI.Outputs;
using BLAZAM.Gui.UI.Outputs.Jobs;
using BLAZAM.Jobs;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Gui.Helper
{
    public static class JobHelpers
    {
        public static DialogParameters<JobResultDialog> ToDialogParameters(this IJob job)
        {
            var parameters = new DialogParameters<JobResultDialog>
            {
                { x => x.Job, job }
            };
            return parameters;
        }
        public static async Task ShowJobDetailsDialogAsync(this IJob job, AppDialogService MessageService)
        {
            await MessageService.ShowMessage<JobResultDialog>(job.ToDialogParameters(), job.Name);
        }
        public static void ShowJobDetailsDialog(this IJob job, AppDialogService MessageService)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            MessageService.ShowMessage<JobResultDialog>(job.ToDialogParameters(), job.Name);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }
}
