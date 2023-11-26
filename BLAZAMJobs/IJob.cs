using BLAZAM.Common.Data;
using BLAZAM.Database.Models.User;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Jobs
{
    public interface IJob
    {

    
        
        DateTime? StartTime { get; set; }
        DateTime? EndTime { get; set; }
        
        DateTime ScheduledRunTime { get; set; }
        IApplicationUserState User { get; set; }
        IList<JobStep> Steps { get; set; }
        IList<JobStep> FailedSteps { get; }

        bool Run();
    }
}