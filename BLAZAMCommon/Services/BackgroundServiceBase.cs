

using BLAZAM.Localization;
using Microsoft.Extensions.Localization;

namespace BLAZAM.Services.Background
{
    public class BackgroundServiceBase : IDisposable
    {
        protected virtual Timer? Timer { get; set; }
        protected virtual TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(10);
        protected bool started { get; set; }
        protected IStringLocalizer<AppLocalization> AppLocalization;

        public BackgroundServiceBase(IStringLocalizer<AppLocalization> appLocalization)
        {
            AppLocalization = appLocalization;
        }




        /// <summary>
        /// Starts this service.
        /// </summary>
        /// <param name="immediate">If false, or not set, the service will wait a 
        /// random time between 15 and 45 seconds after launch.</param>
        public virtual void Start( bool immediate = false)
        {
            if (!started)
            {
                int delay = 0;
                if (!immediate)
                {
                    Random rand = new();
                    int jitter = rand.Next(-15, 15);
                    delay = 30 + jitter;

                }
                if (Interval != TimeSpan.Zero)
                {
                    Timer = new Timer(Execute, null, TimeSpan.FromSeconds(delay), Interval);
                }
                else
                {
                    Task.Delay(delay).ContinueWith((task) => {
                        Execute();
                    });
                }
                started = true;
            }
        }
        /// <summary>
        /// Stops this service from continuing to run.
        /// </summary>
        public virtual void Stop()
        {
            Timer?.Dispose();
            started = false;
        }

        protected virtual void Execute(object? state = null)
        {
            throw new NotImplementedException();
        }

        public virtual void Dispose()
        {
            Timer?.Dispose();
        }
    }
}
