using BLAZAM.Common.Data;
using BLAZAM.Database.Models.User;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    public class Job : IJob
    {
        private DateTime scheduledRunTime = DateTime.Now;

        public string? Title { get; set; }
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

        public Job(string? title = null,IApplicationUserState requestingUser=null)
        {
            Title = title;
            User = requestingUser;
        }

        public async Task<bool> RunAsync()
        {
            return await Task.Run(() => { return Run(); });
        }

        private void TriggerRun(object? state) => Run();

        public bool Run()
        {
            runScheduler?.Dispose();
            FailedSteps.Clear();
            StartTime = DateTime.Now;
            for (int i = 0; i < Steps.Count; i++)
            {
                if (!Steps[i].Run())
                {
                    FailedSteps.Add(Steps[i]);

                }
            }
            EndTime = DateTime.Now;
            return FailedSteps.Count < 1;
        }
    }
}