using BLAZAM.Common.Data;
using BLAZAM.Database.Models.User;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Components;

namespace BLAZAM.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    public class Job : JobStepBase, IJob, IJobStep
    {
        private DateTime scheduledRunTime = DateTime.Now;
        private Timer? runScheduler;

        public string? User { get; set; }

        private IList<IJobStep> _steps = new List<IJobStep>();

        public IList<IJobStep> Steps => _steps;

        public DateTime ScheduledRunTime
        {
            get => scheduledRunTime; set
            {

                scheduledRunTime = value;
                runScheduler = new Timer(TriggerRun, null, (int)(ScheduledRunTime - DateTime.Now).TotalMilliseconds, int.MaxValue);
            }
        }

        public IList<IJobStep> FailedSteps { get; protected set; } = new List<IJobStep>();
        public IList<IJobStep> PassedSteps { get; protected set; } = new List<IJobStep>();


        public Guid Id{ get; set; }
        public bool NestedJob { get; set; } = false;

        public Job(string? title = null, string? requestingUser = null, CancellationTokenSource? externalCancellationToken = null)
        {
            Name = title;
            User = requestingUser;
            if (externalCancellationToken != null)
            {
                cancellationTokenSource = externalCancellationToken;
            }
            Id= Guid.NewGuid();
            JobMonitor.AddJob(this);
        }



        /// <summary>
        /// Used for scheduled triggering
        /// </summary>
        /// <param name="state"></param>
        private void TriggerRun(object? state) => Run();

        public override bool Run()
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
        public void AddStep(IJobStep step)
        {
            if(User != null && step is IJob jobStep)
            {
                jobStep.User = User;
                jobStep.NestedJob = true;
            }
            Steps.Add(step);

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
                if (!Steps[i].Run() && Result != JobResult.Cancelled)
                {
                    FailedSteps.Add(Steps[i]);
                    if (StopOnFailedStep || Steps[i].StopOnFailedStep)
                    {
                        Result = JobResult.Failed;
                        Cancel();
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
            if (Result != JobResult.Cancelled)
            {
                if (FailedSteps.Count > 0)
                {
                    Result = JobResult.Failed;
                }
                else
                {
                    Result = JobResult.Passed;
                }
            }
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

        public void Wait()
        {
            while (Result == JobResult.Running)
            {
                Task.Delay(100).Wait();
            }
        }

        public override void Cancel()
        {
            if (Progress == null || Progress < 100)
            {
                cancellationTokenSource.Cancel();
                foreach (var step in Steps)
                {
                    step.Cancel();
                }
                Result = JobResult.Cancelled;
                // EndTime = DateTime.Now;
                Progress = 100;
            }
        }

        public override bool Equals(object? obj)
        {
            if(obj is IJob job)
            {
                return job.Id.Equals(Id);
            }
            return false;
        }
    }


}