

using BLAZAM.Common.Data;

namespace BLAZAM.Server.Background
{
    

    public class ConnectionMonitor : IConnectionMonitor
    {
        public virtual ServiceConnectionState Status
        {
            get => _connected; set
            {
                if (value == _connected) return;
                _connected = value;
                OnConnectedChanged?.Invoke(value);
            }
        }


        /// <summary>
        /// The time between ticks of the Monitor Timer in milliseconds.
        /// </summary>
        protected virtual int Interval { get; set; } = 20000;
        public AppEvent<ServiceConnectionState>? OnConnectedChanged { get; set; }

        protected bool _monitoring;

        protected Timer? _timer;
        private ServiceConnectionState _connected = ServiceConnectionState.Connecting;


        /// <summary>
        /// Starts a timer to call a local Tick() method on the child
        /// object. The default time is 20 seconds. Adjust the Interval
        /// property before calling to change from 20 seconds.
        /// </summary>
        public void Monitor()
        {
            if (_monitoring == false)
            {
                Status = ServiceConnectionState.Connecting;
                _monitoring = true;
                Task.Run(() => {
                    while (_monitoring)
                    {
                        Task.Delay(Interval).Wait();
                        try
                        {
                            Tick(null);
                        }
                        catch { }

                    }
                });
                
            }
        }
        /// <summary>
        /// Stops the monitoring of this connection
        /// </summary>
        public void Stop()
        {
            if (_timer != null)
            {
                _timer.Dispose();
            }
            _monitoring = false;
        }

        protected virtual void Tick(object? state)
        {
            throw new NotImplementedException();
        }
    }
}
