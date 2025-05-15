using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM
{
    public class AppEvent
    {
        /// <summary>
        /// Called when permission are changed by an admin
        /// </summary>
        public EventHandler Delegate { get; set; }

        /// <summary>
        /// Send event so each user can update permissions
        /// </summary>
        public void Invoke()
        {
            Delegate?.Invoke(null,EventArgs.Empty);

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
        public void Invoke(object sender,T args)
        {
            Delegate?.Invoke(sender, args);

        }
        /// <summary>
        /// Trigger this event
        /// </summary>
        public void Invoke(T args)
        {
            Delegate?.Invoke(null,args);

        }
    }
}
