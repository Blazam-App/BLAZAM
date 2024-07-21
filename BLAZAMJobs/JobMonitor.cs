using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Jobs
{
    public static class JobMonitor
    { 
        public static AppEvent? OnUpdate { get; set; }
        private static List<IJob> Jobs = new List<IJob>();
        public static List<IJob> AllJobs => Jobs.ToList();
        public static List<IJob> RunningJobs => Jobs.Where(x => x.Result == JobResult.Running).ToList();
        public static List<IJob> FailedJobs => Jobs.Where(x => x.Result == JobResult.Failed).ToList();
        public static List<IJob> CompletedJobs => Jobs.Where(x => x.Result == JobResult.Passed).ToList();
        public static List<IJob> PendingJobs => Jobs.Where(x => x.Result == JobResult.NotRun).ToList();
        private static int _maxJobs = 50;


        public static void AddJob(IJob job)
        {
            if(Jobs.Contains(job)) return;
            if (Jobs.Count == _maxJobs) Jobs.RemoveAt(_maxJobs - 1);
            Jobs.Add(job);
            job.OnProgressUpdated += (progress)=>{ OnUpdate?.Invoke(); };
            OnUpdate?.Invoke();
        }

    }
}
