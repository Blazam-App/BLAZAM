using BLAZAM.Common.Data;
using BLAZAM.Database.Models.User;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Jobs
{
    public interface IJob
    {

    
        
        DateTime? StartTime { get; }
        DateTime? EndTime { get; }
        
        DateTime ScheduledRunTime { get; set; }
        IApplicationUserState User { get; set; }
        IList<IJobStep> Steps { get; set; }
        IList<IJobStep> FailedSteps { get; }
        string? Title { get; set; }
        /// <summary>
        /// Returns true if all steps ran without error. 
        /// Returns false if any step failed. 
        /// Returns null if the job hasn't finished.
        /// </summary>
        bool? Result { get; }
        TimeSpan? ElapsedTime { get; }

        bool Run();
        Task<bool> RunAsync();
    }
}