

using BLAZAM.Helpers;
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

        private IList<IJobStep> _steps = [];

        public IList<IJobStep> Steps => _steps;

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
        private void TriggerRun(object? state) => Run();

        public override bool Run()
        {
            if (Identity != null)
            {
                return Identity.Run(() =>
                {
                    return Execute();
                });
            }
            return Execute();

        }
        public void AddStep(IJobStep step)
        {
            if (User != null && step is IJob jobStep)
            {
                jobStep.User = User;
                jobStep.NestedJob = true;
            }
            Steps.Add(step);

        }
        private bool Execute()
        {
            var cancelToken = cancellationTokenSource.Token;

            runScheduler?.Dispose();
            FailedSteps.Clear();
            StartTime = DateTime.Now;
            Result = JobResult.Running;

            Progress = 0;

            if (cancelToken.IsCancellationRequested)
            {
                Cancel();

                return false;
            }

            for (int i = 0; i < Steps.Count; i++)
            {
                Steps[i].OnProgressUpdated += ((val) => { OnProgressUpdated?.Invoke(val); });
                if (!Steps[i].Run() && Result != JobResult.Cancelled)
                {
                    FailedSteps.Add(Steps[i]);
                    if (StopOnFailedStep || Steps[i].StopOnFailedStep)
                    {
                        Result = JobResult.Failed;
                        Cancel();
                        break;

                    }
                }
                else
                {
                    PassedSteps.Add(Steps[i]);

                }
                Progress = 100.0 / Steps.Count * (i + 1);
                if (cancelToken.IsCancellationRequested)
                {
                    Cancel();
                    return false;
                }

            }
            if (Result != JobResult.Cancelled)
            {
                if (FailedSteps.Count > 0)
                {
                    Result = JobResult.Failed;
                }
                else
                {
                    Result = JobResult.Passed;
                }
            }
            EndTime = DateTime.Now;

            Progress = 100;

            return FailedSteps.Count < 1;
        }

        public void Wait()
        {
            while (Result == JobResult.Running)
            {
                Task.Delay(100).Wait();
            }
        }

        public override void Cancel()
        {
            if (Progress == null || Progress < 100)
            {
                cancellationTokenSource.Cancel();
                foreach (var step in Steps)
                {
                    step.Cancel();
                }
                Result = JobResult.Cancelled;
                // EndTime = DateTime.Now;
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