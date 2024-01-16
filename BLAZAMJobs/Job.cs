using BLAZAM.Common.Data;
using BLAZAM.Database.Models.User;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    public class Job : IJob, IJobStep
    {
        private DateTime scheduledRunTime = DateTime.Now;
        public bool StopOnFailedStep { get; set; }
        public string? Name { get; set; }
        public IApplicationUserState User { get; set; }
        private Timer? runScheduler;
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

        public bool? Result
        {
            get
            {
                foreach (var step in Steps)
                {
                    if (step.Result == false) return false;
                    if (step.Result == null) return null;
                }
                return true;
            }
        }

        public Exception Exception { get; protected set; }

        public WindowsImpersonation Identity { get; set; }

        public Job(string? title = null, IApplicationUserState requestingUser = null)
        {
            Name = title;
            User = requestingUser;
        }

        public async Task<bool> RunAsync()
        {
            return await Task.Run(() => { return Run(); });
        }

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
            runScheduler?.Dispose();
            FailedSteps.Clear();
            StartTime = DateTime.Now;
            for (int i = 0; i < Steps.Count; i++)
            {
                if (!Steps[i].Run())
                {
                    FailedSteps.Add(Steps[i]);
                    if (StopOnFailedStep)
                        break;
                }
                else
                {
                    PassedSteps.Add(Steps[i]);
                }
            }
            EndTime = DateTime.Now;
            return FailedSteps.Count < 1;
        }
    }
}