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
        public AppDelegate Delegate { get; set; }

        /// <summary>
        /// Send event so each user can update permissions
        /// </summary>
        public void Invoke()
        {
            Delegate?.Invoke();

        }
    }
    public class AppEvent<T>
    {
        /// <summary>
        /// Called when permission are changed by an admin
        /// </summary>
        public AppDelegate<T> Delegate { get; set; }

        /// <summary>
        /// Send event so each user can update permissions
        /// </summary>
        public void Invoke(T args)
        {
            Delegate?.Invoke(args);

        }
    }
}
