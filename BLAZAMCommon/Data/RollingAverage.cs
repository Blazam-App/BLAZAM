using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Common.Data
{
    public class RollingAverage
    {
        private int memory;
        private List<double> _history;

        public RollingAverage(int memory=10)
        {
            this.memory = memory;
            _history = new List<double>(memory);
        }
        /// <summary>
        /// Adds a value to the average and removes the oldest value if full.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The new average</returns>
        public double AddValue (double value)
        {
            if (_history.Count == memory)
                _history.RemoveAt(0);
            _history.Add(value);
            return _history.Average();
        }

    }
}
