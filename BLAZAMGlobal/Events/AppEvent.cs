namespace BLAZAM.Global.Events
{
    public class AppEvent
    {
        /// <summary>
        /// Listen to this for triggers of this event
        /// </summary>
        public event EventHandler EventHandler;


        /// <summary>
        /// Trigger this event
        /// </summary>
        public void Invoke(object? sender = null)
        {
            if (EventHandler != null)
            {
                EventHandler.Invoke(sender, EventArgs.Empty);
            }

        }
    }
    public class AppEvent<T>
    {
        /// <summary>
        /// Listen to this for triggers of this event
        /// </summary>
        public EventHandler<T> EventHandler { get; set; }

        /// <summary>
        /// Trigger this event
        /// </summary>
        public void Invoke(object sender, T args)
        {
            EventHandler?.Invoke(sender, args);

        }
        /// <summary>
        /// Trigger this event
        /// </summary>
        public void Invoke(T args)
        {
            EventHandler?.Invoke(null, args);

        }
    }
}
