namespace BLAZAM.Global.Events
{
    public class AppEvent
    {
        /// <summary>
        /// Called when permission are changed by an admin
        /// </summary>
        public event EventHandler Delegate;

        /// <summary>
        /// Send event so each user can update permissions
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
