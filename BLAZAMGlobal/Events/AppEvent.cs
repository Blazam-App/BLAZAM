namespace BLAZAM.Global.Events
{
    public class AppEvent
    {
        /// <summary>
        /// Listen to this for triggers of this event
        /// </summary>
        public event EventHandler Delegate;


        /// <summary>
        /// Trigger this event
        /// </summary>
        public void Invoke(object? sender = null)
        {
            if (Delegate != null)
            {
                Delegate.Invoke(sender, EventArgs.Empty);
            }

        }
    }
    public class AppEvent<T>
    {
        /// <summary>
        /// Listen to this for triggers of this event
        /// </summary>
        public EventHandler<T> Delegate { get; set; }

        /// <summary>
        /// Trigger this event
        /// </summary>
        public void Invoke(object sender, T args)
        {
            Delegate?.Invoke(sender, args);

        }
        /// <summary>
        /// Trigger this event
        /// </summary>
        public void Invoke(T args)
        {
            Delegate?.Invoke(null, args);

        }
    }
}
