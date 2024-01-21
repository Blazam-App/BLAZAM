using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Interfaces;
using Microsoft.AspNetCore.Components;
using MudBlazor.Services;
using System.Runtime.CompilerServices;

namespace BLAZAM.Jobs
{
    public class JobStep : JobStepBase, IJobStep 
    {
        public Func<bool> Action { get; set; }

        public JobStep(string name, Func<bool> action)
        {
            Name = name;
            Action = action;
        }

        public JobStep(string name, Func<bool> action, WindowsImpersonation identity) : this(name, action)
        {
            Identity = identity;
        }
        public void Cancel()
        {
            if (Progress < 100)
            {
                cancellationTokenSource.Cancel();
                EndTime = DateTime.Now;
                Result = JobResult.Cancelled;
                Progress = 100;
            }
        }

        public override bool Run()
        {
            var cancelToken = cancellationTokenSource.Token;
            try
            {
                if (cancelToken.IsCancellationRequested)
                {
                    Cancel();
                    return false;
                }

                StartTime = DateTime.Now;
                Result = JobResult.Running;
                if (Progress == 0)
                {
                    OnProgressUpdated?.Invoke(0);
                }
                else
                {
                    Progress = 0;
                }
                var actionResult = Action.Invoke();
                if (cancelToken.IsCancellationRequested)
                {
                    Cancel();
                    return false;
                }
                if (actionResult)
                {
                    Result = JobResult.Passed;
                }
                else
                {
                    Result = JobResult.Failed;

                }
                EndTime = DateTime.Now;
                Progress = 100;
                return Result == JobResult.Passed;

            }
            catch (Exception ex)
            {
                Result = Result = JobResult.Failed;
                Exception = ex;
                EndTime = DateTime.Now;
                Progress = 100;

                return false;
            }
        }
    }
}