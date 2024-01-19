using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Interfaces;
using Microsoft.AspNetCore.Components;
using MudBlazor.Services;
using System.Runtime.CompilerServices;

namespace BLAZAM.Jobs
{
    public class JobStep : IJobStep 
    {
        private double progress=0;
        private CancellationTokenSource cancellationTokenSource=new();

        public virtual string Name { get; set; }
        public Exception Exception { get; private set; }
        public WindowsImpersonation Identity { get; set; }
        public Func<bool> Action { get; set; }

        public JobResult Result { get; set; } = JobResult.NotRun;
        public AppEvent<double> OnProgressUpdated { get; set; }
        public double Progress
        {
            get => progress; set
            {
                if (value == progress) return;
                progress = value;
                OnProgressUpdated?.Invoke(progress);
            }
        }

        public DateTime? StartTime {get;set; }

        public DateTime? EndTime { get; set; }

        public TimeSpan? ElapsedTime
        {
            get
            {
                if (EndTime == null) return null;
                return EndTime - StartTime;
            }
        }

        public void Cancel()
        {
            if (Progress < 100)
            {
                cancellationTokenSource.Cancel();

                Result = JobResult.Cancelled;
                Progress = 100;
            }
        }

        public JobStep(string name, Func<bool> action)
        {
            Name = name;
            Action = action;
        }

        public JobStep(string name, Func<bool> action, WindowsImpersonation identity) : this(name, action)
        {
            Identity = identity;
        }

        public bool Run()
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
                return Result==JobResult.Passed;

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