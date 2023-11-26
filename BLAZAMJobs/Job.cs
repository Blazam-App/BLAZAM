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
        public IApplicationUserState User { get; set; }
        public IList<JobStep> Steps { get; set; }=new List<JobStep>();
        public DateTime ScheduledRunTime { get; set; } = DateTime.Now;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public IList<JobStep> FailedSteps { get; private set; }= new List<JobStep>();


        public async Task<bool> RunAsync()
        {
            return await Task.Run(() => { return Run(); });
        }
        public bool Run()
        {
            FailedSteps.Clear();    
            StartTime = DateTime.Now;
            for(int i = 0; i < Steps.Count; i++)
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