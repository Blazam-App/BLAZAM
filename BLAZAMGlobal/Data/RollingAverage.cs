namespace BLAZAM.Global.Data
{
    public class RollingAverage
    {
        private int memory;
        private List<double> _history;

        /// <summary>
        /// Returns the total samples in this average
        /// </summary>
        public int Count => _history.Count;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memory"></param>
        public RollingAverage(int memory = 10)
        {
            this.memory = memory;
            _history = new List<double>(memory);
        }
        /// <summary>
        /// Adds a value to the average and removes the oldest value if full.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The new average</returns>
        public void AddValue(double value)
        {
            if (_history.Count == memory)
            {
                _history.RemoveAt(0);
            }

            _history.Add(value);

        }
        public double GetAverage(int roundedDecimalPlaces = 0)
        {
            if (_history.Count == 0)
            {
                return 0;
            }

            if (roundedDecimalPlaces > 0)
            {
                return Math.Round(_history.Average(), roundedDecimalPlaces);
            }
            return _history.Average();
        }

    }
}
