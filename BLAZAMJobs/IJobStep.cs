using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Interfaces;

namespace BLAZAM.Jobs
{
    /// <summary>
    /// An action step that can be inserted into a <see cref="IJob"/>
    /// </summary>
    public interface IJobStep : IProgressTracker<double>
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
        /// The name of this step
        /// </summary>
        string? Name { get; set; }

        /// <summary>
        /// The result of this step
        /// </summary>
        JobResult Result { get; }

        /// <summary>
        /// Executes the actions of this step
        /// </summary>
        /// <returns>True if the action complete without exceptions, otherwise false</returns>
        bool Run();

        DateTime? StartTime { get; }
        DateTime? EndTime { get; }
        TimeSpan? ElapsedTime { get; }

        void Cancel();

    }
}