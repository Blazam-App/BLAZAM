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

     
        public bool StopOnFailedStep { get; set; }

     

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
      
        public IList<IJobStep> FailedSteps { get; protected set; } = new List<IJobStep>();
        public IList<IJobStep> PassedSteps { get; protected set; } = new List<IJobStep>();

     
       

        public Job(string? title = null, IApplicationUserState requestingUser = null)
        {
            Name = title;
            User = requestingUser;
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