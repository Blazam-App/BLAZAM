using BLAZAM.Common.Data;
using BLAZAM.Database.Models.User;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Components;

namespace BLAZAM.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    public class Job : IJob, IJobStep
    {
        private DateTime scheduledRunTime = DateTime.Now;
        private Timer? runScheduler;
        private double progress;

        private CancellationTokenSource cancellationTokenSource = new();

        public bool StopOnFailedStep { get; set; }

        public string? Name { get; set; }

        public IApplicationUserState User { get; set; }

        public IList<IJobStep> Steps { get; set; } = new List<IJobStep>();

        public DateTime ScheduledRunTime
        {
            get => scheduledRunTime; set
            {

                scheduledRunTime = value;
                runScheduler = new Timer(TriggerRun, null, (int)(ScheduledRunTime - DateTime.Now).TotalMilliseconds, int.MaxValue);
            }
        }
        public DateTime? StartTime { get; protected set; }
        public DateTime? EndTime { get; protected set; }
        public TimeSpan? ElapsedTime
        {
            get
            {
                if (EndTime == null) return null;
                return EndTime - StartTime;
            }
        }
        public IList<IJobStep> FailedSteps { get; protected set; } = new List<IJobStep>();
        public IList<IJobStep> PassedSteps { get; protected set; } = new List<IJobStep>();

        public JobResult Result { get; set; }

        public Exception Exception { get; protected set; }

        public WindowsImpersonation Identity { get; set; }
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

        public Job(string? title = null, IApplicationUserState requestingUser = null)
        {
            Name = title;
            User = requestingUser;
        }

        public async Task<bool> RunAsync()
        {
            return await Task.Run(() => { return Run(); });
        }

        /// <summary>
        /// Used for scheduled triggering
        /// </summary>
        /// <param name="state"></param>
        private void TriggerRun(object? state) => Run();

        public bool Run()
        {
            if (Identity != null)
            {
                return Identity.Run(() =>
                {
                    return Execute();
                });
            }
            return Execute();

        }

        private bool Execute()
        {
            var cancelToken = cancellationTokenSource.Token;

            runScheduler?.Dispose();
            FailedSteps.Clear();
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
            if (cancelToken.IsCancellationRequested)
            {
                Cancel();
                return false;
            }

            for (int i = 0; i < Steps.Count; i++)
            {
                Steps[i].OnProgressUpdated += ((val) => { OnProgressUpdated?.Invoke(val); });
                if (!Steps[i].Run())
                {
                    FailedSteps.Add(Steps[i]);
                    if (StopOnFailedStep)
                    {
                        Result = JobResult.Failed;
                        break;

                    }
                }
                else
                {
                    PassedSteps.Add(Steps[i]);

                }
                Progress = 100.0 / Steps.Count * (i + 1);
                if (cancelToken.IsCancellationRequested)
                {
                    Cancel();
                    return false;
                }

            }
            if (FailedSteps.Count > 0)
            {
                Result = JobResult.Failed;
            }
            else { Result = JobResult.Passed; }
            EndTime = DateTime.Now;
            if (Progress == 100)
            {
                OnProgressUpdated?.Invoke(Progress);

            }
            else
            {
                Progress = 100;
            }
            return FailedSteps.Count < 1;
        }

        public void Cancel()
        {
            if (Progress < 100)
            {
                cancellationTokenSource.Cancel();
                foreach (var step in Steps)
                {
                    step.Cancel();
                }
                Result = JobResult.Cancelled;

                Progress = 100;
            }
        }


    }

    
}