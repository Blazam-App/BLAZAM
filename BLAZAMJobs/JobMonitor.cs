

namespace BLAZAM.Jobs
{
    public static class JobMonitor
    {
        public static AppDelegate? OnUpdate { get; set; }
        private static List<IJob> Jobs = [];
        public static List<IJob> AllJobs => Jobs;
        public static IEnumerable<IJob> RunningJobs => Jobs.Where(x => x.Result == JobResult.Running);
        public static IEnumerable<IJob> FailedJobs => Jobs.Where(x => x.Result == JobResult.Failed);
        public static IEnumerable<IJob> CompletedJobs => Jobs.Where(x => x.Result == JobResult.Passed);
        public static IEnumerable<IJob> PendingJobs => Jobs.Where(x => x.Result == JobResult.NotRun);
        private static int _maxJobs = 200;


        public static void AddJob(IJob job)
        {
            if (Jobs.Contains(job))
            {
                return;
            }

            if (Jobs.Count == _maxJobs)
            {
                Jobs.RemoveAt(_maxJobs - 1);
            }

            Jobs.Add(job);
            job.OnProgressUpdated += (progress) => { OnUpdate?.Invoke(); };
            OnUpdate?.Invoke();
        }

    }
}
