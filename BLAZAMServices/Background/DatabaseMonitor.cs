using BLAZAM.Database.Context;

namespace BLAZAM.Services.Background
{
    public class DatabaseMonitor : ConnectionMonitor
    {
        private IDatabaseContext _context;


        public DatabaseMonitor(IDatabaseContext context)
        {
            Interval = 10000;
            _context = context;
            Task.Delay(1000).ContinueWith((oob) => { Tick(null); });
            Monitor();
        }

        protected override void Tick(object? state)
        {

            Status = _context.Status;

        }

    }
}