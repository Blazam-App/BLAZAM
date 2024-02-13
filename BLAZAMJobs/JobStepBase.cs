using BLAZAM.Common.Data;

namespace BLAZAM.Jobs
{
    public class JobStepBase : IJobStepBase
    {
        protected  CancellationTokenSource cancellationTokenSource = new();
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
        public virtual AppEvent<double?> OnProgressUpdated { get; set; }
        public virtual double? Progress
        {
            get => progress; set
            {
                if (value.Value == progress) return;
                progress = value.Value;
                OnProgressUpdated?.Invoke(progress);
            }
        }

        public virtual JobResult Result { get; protected set; } = JobResult.NotRun;

        public virtual DateTime? StartTime { get; protected set; }


        public virtual bool StopOnFailedStep { get; set; }

        public virtual async Task<bool> RunAsync()
        {
            return await Task.Run(() => { return Run(); });
        }
        public virtual bool Run()
        {
            throw new ApplicationException("This step contains no action.");
        }

        public virtual void Cancel()
        {
            throw new NotImplementedException("This job component has not implemented cancel.");
        }
    }
}