using BLAZAM.Common.Data;

namespace BLAZAM.Jobs
{

    public class JobStepBase : IJobStepBase
    {
        protected CancellationTokenSource cancellationTokenSource = new();
        private double? progress = null;

        public virtual TimeSpan? ElapsedTime
        {
            get
            {
                if (Result == JobResult.Running && StartTime != null) return DateTime.Now - StartTime;
                if (EndTime == null) return null;
                return EndTime - StartTime;
            }
        }

        public virtual DateTime? EndTime { get; protected set; }
        public virtual Exception Exception { get; protected set; }
        public virtual WindowsImpersonation Identity { get; set; }

        public virtual string? Name { get; set; }
        public virtual AppDelegate<double?> OnProgressUpdated { get; set; }
        public virtual double? Progress
        {
            get => progress; set
            {
                if (value != null && progress != null && Math.Abs((value - (float)progress).Value) < 0.1) return;
                if (value != null)
                {
                    value = Math.Clamp(value.Value, 0, 100);
                }
                progress = value;

                OnProgressUpdated?.Invoke(progress);
            }
        }
        private JobResult _result { get; set; } = JobResult.NotRun;
        public virtual JobResult Result
        {
            get => _result; protected set
            {
                if (value == _result) return;

                _result = value;

                OnProgressUpdated?.Invoke(Progress);
            }
        }

        public virtual DateTime? StartTime { get; protected set; }


        public virtual bool StopOnFailedStep { get; set; }

        /// <summary>
        /// Gets or sets the thread priority for the job and its steps when run asynchronously.
        /// </summary>
        public System.Threading.ThreadPriority ThreadPriority { get; set; } = System.Threading.ThreadPriority.Normal;

        public virtual async Task<bool> RunAsync()
        {
            // Set thread priority for the task's thread
            if (ThreadPriority != ThreadPriority.Normal)
            {
                Thread thread = new Thread(this.RunBackground);
                thread.Name = "RunAsyncJob";
                thread.Priority = ThreadPriority;
                thread.Start();
                while (Result != JobResult.Passed && Result != JobResult.Failed && Result != JobResult.Cancelled)
                {
                    await Task.Delay(500);
                }
                return Result == JobResult.Passed;
            }
            else
            {
                return await Task.Run(() =>
                {
                    return Run();
                });
            }
        }
        private void RunBackground()
        {
            _ = Run();
        }
        public virtual bool Run()
        {
            throw new AppException("This step contains no action.");
        }

        public virtual void Cancel()
        {
            throw new NotImplementedException("This job component has not implemented cancel.");
        }
    }
}