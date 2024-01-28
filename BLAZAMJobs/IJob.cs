using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Interfaces;
using BLAZAM.Database.Models.User;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Jobs
{
    /// <summary>
    /// A flexible multi step Job that can have actions as trackable steps.
    /// </summary>
    public interface IJob:IProgressTracker<double?>
    {

    
        /// <summary>
        /// The time execution of this job started
        /// </summary>
        DateTime? StartTime { get; }

        /// <summary>
        /// The time execution finished for this job
        /// </summary>
        /// <remarks>
        /// Even on error, this value is set and indicates when the error occured</remarks>
        DateTime? EndTime { get; }

        /// <summary>
        /// The total time of execution since the job started
        /// </summary>
        /// <remarks>
        /// Returns null if the job has not yet been started
        /// </remarks>
        TimeSpan? ElapsedTime { get; }

        /// <summary>
        /// The time to run this job. Execution will wait until this time arrives.
        /// </summary>
        DateTime ScheduledRunTime { get; set; }

        /// <summary>
        /// The triggering username
        /// </summary>
        string? User { get; set; }

        /// <summary>
        /// The steps associated with this job, in order.
        /// </summary>
        /// <remarks>
        /// Some steps may be <see cref="Job"/>'s themselves
        /// </remarks>
        IList<IJobStep> Steps { get; set; }

        /// <summary>
        /// A list of all failed steps
        /// <para>
        /// Empty if all steps succeed
        /// </para>
        /// <para>
        /// Will only contain first error if <see cref="StopOnFailedStep"/> is true
        /// </para>
        /// 
        /// </summary>
        /// 
        IList<IJobStep> FailedSteps { get; }
        /// <summary>
        /// The name of this job
        /// </summary>
        string? Name { get; set; }
        /// <summary>
        /// Returns true if all steps ran without error. 
        /// Returns false if any step failed. 
        /// Returns null if the job hasn't finished.
        /// </summary>
        JobResult Result { get; }
        /// <summary>
        /// If true, the job will stop of first error, and cancel all remaining steps
        /// </summary>
        bool StopOnFailedStep { get; set; }

        /// <summary>
        /// All the steps that executed without exception
        /// </summary>
        IList<IJobStep> PassedSteps { get; }

        /// <summary>
        /// Run this job immediately
        /// </summary>
        /// <remarks>
        /// Calling this overrides any schedule that may have been set
        /// </remarks>
        /// <returns></returns>
        bool Run();

        /// <summary>
        /// Run this job immediately and asynchronously
        /// </summary>
        /// <remarks>
        /// Calling this overrides any schedule that may have been set
        /// </remarks>
        /// <returns></returns>
        Task<bool> RunAsync();


        /// <summary>
        /// Cancels this job after the current step finishes if execution already started
        /// </summary>
        void Cancel();
        void Wait();
    }
}