

using BLAZAM.Helpers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

namespace BLAZAM.Jobs
{

    /// <summary>
    /// A flexible multi step Job that can have actions as trackable steps.
    /// </summary>
    public class Job : JobStepBase, IJob, IJobStep, IEquatable<Job?>
    {
        private DateTime scheduledRunTime = DateTime.Now;
        private Timer? runScheduler;

        public string? User { get; set; }

        private ConcurrentQueue<IJobStep> _queue = new ConcurrentQueue<IJobStep>();

        public IList<IJobStep> Steps { get; } = [];

        public DateTime ScheduledRunTime
        {
            get => scheduledRunTime; set
            {

                scheduledRunTime = value;
                runScheduler = new Timer(TriggerRun, null, (int)(ScheduledRunTime - DateTime.Now).TotalMilliseconds, int.MaxValue);
            }
        }

        public IList<IJobStep> FailedSteps { get; protected set; } = new List<IJobStep>();
        public IList<IJobStep> PassedSteps { get; protected set; } = new List<IJobStep>();


        public Guid Id { get; set; }
        public bool NestedJob { get; set; } = false;

        public Job(string? title = null, string? requestingUser = null, CancellationTokenSource? externalCancellationToken = null)
        {
            Name = title;
            User = requestingUser;
            if (User.IsNullOrEmpty())
            {
                User = GetCallingClassName();
            }
            if (externalCancellationToken != null)
            {
                cancellationTokenSource = externalCancellationToken;
            }
            Id = Guid.NewGuid();
            JobMonitor.AddJob(this);
        }

        private string GetCallingClassName()
        {
            var stackTrace = new StackTrace();
            MethodBase? method = null;
            // Start from frame 2 to skip Job and GetCallingClassName


            for (int i = 2; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                method = frame?.GetMethod();

                // Check for async methods and compiler-generated types
                if (
                    method?.DeclaringType?.Name.StartsWith("<") == true
                    || method?.DeclaringType?.Name.StartsWith("AsyncStateMachineBox") == true
                    || method?.DeclaringType?.Name.StartsWith("WorkerThrea") == true
                    || method?.DeclaringType?.Name.StartsWith("Task") == true
                    || method?.DeclaringType?.Name.StartsWith("ExecutionContext") == true
                    || method?.DeclaringType?.Name.StartsWith("AsyncTaskMethodBuilder") == true
                    || method?.DeclaringType?.Name.StartsWith("ThreadPoolWorkQueue") == true
                    || method?.DeclaringType?.Name.StartsWith("AwaitTaskContinuation") == true)
                {
                    continue;
                }

                // Found a non-async, non-compiler-generated method
                break;
            }

            return method?.DeclaringType?.Name ?? "System";
        }

        /// <summary>
        /// Used for scheduled triggering
        /// </summary>
        /// <param name="state"></param>
        private void TriggerRun(object? state) => RunStep();

        public async Task<bool> RunAsync() => await Task.Run(Run);

        public bool Run()
        {
            JobBroker.GetRunToken();
            var runSuccess = RunStep();
            JobBroker.ReleaseRunToken();
            return runSuccess;
        }
        public override bool RunStep()
        {

            if (Identity != null)
            {
                return Identity.Run(Execute);
            }
            else
            {
                return Execute();
            }

        }
        public void AddStep(IJobStep step)
        {
            if (User != null && step is IJob jobStep)
            {
                jobStep.User = User;
                jobStep.NestedJob = true;
            }

            _queue.Enqueue(step);
            Steps.Add(step);
            if (Result == JobResult.Passed || (Result == JobResult.Failed && !StopOnFailedStep))
            {
                ExecuteStep(step);
            }
        }
        private bool Execute()
        {

            var cancelToken = cancellationTokenSource.Token;

            runScheduler?.Dispose();
            FailedSteps.Clear();
            PassedSteps.Clear();
            StartTime = DateTime.Now;
            Result = JobResult.Running;

            int completedStepsCount = 0;

            // Process steps from the queue until it's empty or cancelled/failed
            while (_queue.TryDequeue(out var step))
            {
                if (cancelToken.IsCancellationRequested)
                {
                    // Re-enqueue the step if cancelled before running it
                    _queue.Enqueue(step); // Put it back in case the job is resumed or retried
                    Cancel(); // Call Cancel to propagate cancellation
                    return false; // Exit execution
                }

                step.OnProgressUpdated += ((val) => { OnProgressUpdated?.Invoke(val); });

                if (!ExecuteStep(step))
                {
                    Cancel();
                    break;
                }


                completedStepsCount++;

                // Update progress based on completed steps vs. current total steps in the bag
                // This progress can be non-linear if steps are added during execution.
                if (Steps.Count > 0)
                {
                    Progress = (double)completedStepsCount / Steps.Count * 100.0;
                }
                else
                {
                    Progress = 100.0; // Handle case where no steps were initially added but job is run
                }
            }




            // Finalize job result after the loop
            if (Result == JobResult.Running) // Only set final result if not already cancelled or failed by StopOnFailedStep
            {
                if (FailedSteps.Count > 0)
                {
                    Result = JobResult.Failed;
                }
                else if (Steps.All(s => s.Result == JobResult.Passed))
                {
                    Result = JobResult.Passed;
                }
                else
                {
                    // If the loop finished without explicit failure or cancellation,
                    // but not all steps are marked as Passed, this might indicate an issue
                    // or that some steps are still running (though synchronous Run() should prevent this).
                    // Treat as failed or incomplete.
                    Result = JobResult.Failed;
                }
            }


            EndTime = DateTime.Now;

            // Ensure progress is 100 at the end, even if failed or cancelled
            Progress = 100;



            return FailedSteps.Count < 1 && Result != JobResult.Cancelled;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="step"></param>
        /// <returns>False if the job should stop, true if it should continue.</returns>
        private bool ExecuteStep(IJobStep step)
        {
            Result = JobResult.Running;
            if (!step.RunStep() && step.Result != JobResult.Cancelled)
            {
                FailedSteps.Add(step);
                if (StopOnFailedStep || step.StopOnFailedStep)
                {
                    Result = JobResult.Failed;
                    return false; // Stop processing further steps on failure
                }
            }
            else if (step.Result == JobResult.Passed)
            {
                PassedSteps.Add(step);
            }
            else if (step.Result == JobResult.Cancelled)
            {
                Result = JobResult.Cancelled;
                return false; // Stop processing further steps on failure
            }
            return true;
        }
        public void Wait()
        {
            while (Result == JobResult.Running)
            {
                Task.Delay(100).Wait();
            }
        }
        public async Task WaitAsync()
        {
            while (Result == JobResult.Running)
            {
                await Task.Delay(100);
            }
        }

        public override void Cancel()
        {
            // Prevent multiple cancellations or cancelling after completion
            if (Result == JobResult.Running || Result == JobResult.NotRun || Result == JobResult.Failed)
            {
                cancellationTokenSource.Cancel();
                // Propagate cancellation to individual steps (Cancel method should handle if step is running)
                foreach (var step in _queue) // Iterate the ConcurrentBag for cancellation propagation
                {
                    step.Cancel();
                }
                Result = JobResult.Cancelled;
                // EndTime might be set by the executing task, but setting it here
                // provides an immediate timestamp for the cancellation request.
                if (EndTime == null)
                {
                    EndTime = DateTime.Now;
                }
                // Set progress to 100 immediately on cancellation
                Progress = 100;

            }
        }




        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Job);
        }

        public bool Equals(Job? other)
        {
            return other is not null &&
                   Id.Equals(other.Id);
        }

        public static bool operator ==(Job? left, IJob? right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Job? left, IJob? right)
        {
            return !(left == right);
        }

        public static bool operator ==(Job? left, Job? right)
        {
            return EqualityComparer<Job>.Default.Equals(left, right);
        }

        public static bool operator !=(Job? left, Job? right)
        {
            return !(left == right);
        }
    }


}