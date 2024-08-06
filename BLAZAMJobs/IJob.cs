using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Interfaces;
using BLAZAM.Database.Models.User;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Jobs
{
    /// <summary>
    /// A flexible multi step Job that can have actions as trackable steps.
    /// </summary>
    public interface IJob : IJobStepBase
    {


        bool NestedJob { get; set; }
        Guid Id { get; }
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
        IList<IJobStep> Steps { get; }

        /// <summary>
        /// A list of all failed steps
        /// <para>
        /// Empty if all steps succeed
        /// </para>
        /// <para>
        /// Will only contain the first error if <see cref="StopOnFailedStep"/> is true
        /// </para>
        /// 
        /// </summary>
        /// 
        IList<IJobStep> FailedSteps { get; }



        /// <summary>
        /// All the steps that executed without exception
        /// </summary>
        IList<IJobStep> PassedSteps { get; }

        void AddStep(IJobStep step);



        /// <summary>
        /// Waits for the job to finish execution synchronously.
        /// </summary>
        void Wait();
    }
}