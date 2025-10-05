using BLAZAM.Gui.UI.Outputs.Jobs;
using BLAZAM.Jobs;
using MudBlazor;

namespace BLAZAM.Gui.Helper
{
    public static class JobHelpers
    {
        public static IJob TestJob
        {
            get
            {
                var job = new Job("Test Job");
                var job2 = new Job("Nested Job");
                var step1 = new JobStep("Regular Step Passes", (step) => { Task.Delay(2000).Wait(); return true; });
                var step2 = new JobStep("Regular Step Fails", (step) => { Task.Delay(2000).Wait(); return false; });
#pragma warning disable CS0162 // Unreachable code detected
                var step3 = new JobStep("Regular Step Throws", (step) => { Task.Delay(2000).Wait(); throw new AppException("Test exception"); return false; });
#pragma warning restore CS0162 // Unreachable code detected
                var step4 = new JobStep("Nested Step Passes", (step) => { Task.Delay(2000).Wait(); return true; });
                var step5 = new JobStep("Nested Step Fails", (step) => { Task.Delay(2000).Wait(); return false; });
#pragma warning disable CS0162 // Unreachable code detected
                var step6 = new JobStep("Nested Step Throws", (step) => { Task.Delay(2000).Wait(); throw new AppException("Test exception"); return false; });
#pragma warning restore CS0162 // Unreachable code detected

                job.AddStep(step1);
                job.AddStep(step2);
                job.AddStep(step3);
                job2.AddStep(step4);
                job2.AddStep(step5);
                job2.AddStep(step6);
                job.AddStep(job2);
                return job;
            }
        }

        public static DialogParameters<JobResultDialog> ToDialogParameters(this IJob job)
        {
            var parameters = new DialogParameters<JobResultDialog>
            {
                { x => x.Job, job }
            };
            return parameters;
        }
        /// <summary>
        /// Opens a <see cref="JobResultDialog"/> for this <see cref="IJob"/> asynchronously
        /// </summary>
        /// <param name="job"></param>
        /// <param name="MessageService"></param>
        /// <returns></returns>
        public static async Task ShowJobDetailsDialogAsync(this IJob job, AppDialogService MessageService)
        {
            await MessageService.ShowMessage<JobResultDialog>(job.ToDialogParameters(), job.Name);
        }
        /// <summary>
        /// Opens a <see cref="JobResultDialog"/> for this <see cref="IJob"/>
        /// </summary>
        /// <param name="job"></param>
        /// <param name="MessageService"></param>
        /// <returns></returns>
        public static void ShowJobDetailsDialog(this IJob job, AppDialogService MessageService)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            MessageService.ShowMessage<JobResultDialog>(job.ToDialogParameters(), job.Name);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }
}
