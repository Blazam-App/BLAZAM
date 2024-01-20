using BLAZAM.Common.Data;

namespace BLAZAM.Jobs
{
    public class JobStepBase
    {
        protected CancellationTokenSource cancellationTokenSource = new();
        private double progress = 0;

        public TimeSpan? ElapsedTime
        {
            get
            {
                if (EndTime == null) return null;
                return EndTime - StartTime;
            }
        }

        public DateTime? EndTime { get; protected set; }
        public Exception Exception { get; protected set; }
        public WindowsImpersonation Identity { get; set; }

        public virtual string? Name { get; set; }
        public AppEvent<double> OnProgressUpdated { get; set; }
        public double Progress
        {
            get => progress; set
            {
                if (value == progress) return;
                progress = value;
                OnProgressUpdated?.Invoke(progress);
            }
        }

        public JobResult Result { get; protected set; } = JobResult.NotRun;

        public DateTime? StartTime { get; protected set; }

        public async Task<bool> RunAsync()
        {
            return await Task.Run(() => { return Run(); });
        }
        public virtual bool Run()
        {
            throw new NotImplementedException();
        }
    }
}