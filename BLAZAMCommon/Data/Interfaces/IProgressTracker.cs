using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Common.Data.Interfaces
{
    /// <summary>
    /// Provides a progress variable and an event when updated
    /// </summary>
    /// <typeparam name="TNumber">A numerical type</typeparam>
    public interface IProgressTracker<TNumber>
    {
        /// <summary>
        /// Called when the progress changes
        /// </summary>
        /// <remarks>
        /// Sending the same value to <see cref="Progress"/> 
        /// will not result in this being called </remarks>
        AppEvent<TNumber?> OnProgressUpdated { get; set; }
        /// <summary>
        /// The current progress between 0 and 100
        /// </summary>
        TNumber? Progress { get; set; }
    }
}
