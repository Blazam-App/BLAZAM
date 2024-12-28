using BLAZAM.Database.Context;
using BLAZAM.Services.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Services.Background
{
    public class BackgroundServiceBase : IDisposable
    {
        protected virtual IAppDatabaseFactory dbFactory { get; }
        protected virtual Timer? Timer { get; set; }
        protected virtual TimeSpan Interval{ get; set; } = TimeSpan.FromMinutes(10);
        protected static bool started { get; set; }
        public BackgroundServiceBase(IAppDatabaseFactory dbFactory)
        {
            this.dbFactory = dbFactory;

        }
        public virtual void Start(bool immediate=false)
        {
            if (!started)
            {
                int delay=0;
                if (!immediate)
                {
                    Random rand = new Random();
                   int jitter = rand.Next(-15, 15);
                    delay = 30 + jitter;

                }
                Timer = new Timer(Execute, null, TimeSpan.FromSeconds(delay), Interval);
                started = true;
            }
        }

        public virtual void Stop()
        {
            Timer?.Dispose();
            started = false;
        }

        protected virtual void Execute(object? state=null)
        {
            throw new NotImplementedException();    
        }

        public virtual void Dispose()
        {
            Timer?.Dispose();
        }
    }
}
