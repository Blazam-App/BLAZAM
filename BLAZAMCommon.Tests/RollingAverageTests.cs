using BLAZAM.Common.Data;
using BLAZAM.Helpers;

namespace BLAZAMCommon.Tests
{
    public class RollingAverageTests
    {
        /// <summary>
        /// This really has no reason to ever change,
        /// but if it does, update the CanDecrypt
        /// method's test cipherString
        /// 
        /// </summary>
        /// <remarks>Value: thisisaseedkeystring</remarks>

        public RollingAverageTests()
        {

        }
        [Fact]
        public void ReportsZeroWhenEmpty()
        {
            var ra = new RollingAverage();
            Assert.Equal(0, ra.GetAverage());
        }

        [Fact]
        public void ReportsAccurateAverage()
        {
            var ra2 = new RollingAverage();
            ra2.AddValue(5);
            ra2.AddValue(10);
            ra2.AddValue(15);
            ra2.AddValue(20);
            ra2.AddValue(25);

            Assert.Equal(15, ra2.GetAverage());
        }
        [Fact]
        public void CanAddValue()
        {
            var ra3 = new RollingAverage();
            ra3.AddValue(5);
            Assert.Equal(5, ra3.GetAverage());
        }

    }
}
