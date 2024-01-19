using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Interfaces;
using BLAZAM.Database.Models.User;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Jobs
{
    public interface IJob:IProgressTracker<double>
    {

    
        
        DateTime? StartTime { get; }
        DateTime? EndTime { get; }
        TimeSpan? ElapsedTime { get; }

        DateTime ScheduledRunTime { get; set; }
        IApplicationUserState User { get; set; }
        IList<IJobStep> Steps { get; set; }
        IList<IJobStep> FailedSteps { get; }
        string? Name { get; set; }
        /// <summary>
        /// Returns true if all steps ran without error. 
        /// Returns false if any step failed. 
        /// Returns null if the job hasn't finished.
        /// </summary>
        JobResult Result { get; }
        bool StopOnFailedStep { get; set; }
        IList<IJobStep> PassedSteps { get; }

        bool Run();
        Task<bool> RunAsync();

        void Cancel();
    }
}