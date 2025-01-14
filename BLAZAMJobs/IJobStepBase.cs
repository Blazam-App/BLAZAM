using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Interfaces;

namespace BLAZAM.Jobs
{
    public interface IJobStepBase : IProgressTracker<double?>
    {

        /// <summary>
        /// The exception that caused this step to fail, if any
        /// </summary>
        Exception? Exception { get; }

        /// <summary>
        /// The Windows Identity to run this step as
        /// </summary>
        WindowsImpersonation Identity { get; set; }

        /// <summary>
        /// The name of this job/step
        /// </summary>
        string? Name { get; set; }
        /// <summary>
        /// Returns true if all actions ran without error. 
        /// Returns false if any actions failed. 
        /// Returns null if the action hasn't finished.
        /// </summary>
        JobResult Result { get; }


        /// <summary>
        /// The time execution of this job started
        /// </summary>
        DateTime? StartTime { get; }

        /// <summary>
        /// The time execution finished for this job
        /// </summary>
        /// <remarks>
        /// Even on error, this value is set and indicates when the error occurred</remarks>
        DateTime? EndTime { get; }

        /// <summary>
        /// The total time of execution since the job started
        /// </summary>
        /// <remarks>
        /// Returns null if the job has not yet been started
        /// </remarks>
        TimeSpan? ElapsedTime { get; }

        /// <summary>
        /// If true, the job will stop of first error, and cancel all remaining steps
        /// </summary>
        bool StopOnFailedStep { get; set; }



        /// <summary>
        /// Run this job/step immediately
        /// </summary>
        /// <remarks>
        /// Calling this overrides any schedule that may have been set
        /// </remarks>
        /// <returns></returns>
        bool Run();

        /// <summary>
        /// Run this job/step immediately and asynchronously
        /// </summary>
        /// <remarks>
        /// Calling this overrides any schedule that may have been set
        /// </remarks>
        /// <returns></returns>
        Task<bool> RunAsync();

        /// <summary>
        /// Cancels this job/step after the current action finishes if execution already started
        /// </summary>
        void Cancel();
    }
}