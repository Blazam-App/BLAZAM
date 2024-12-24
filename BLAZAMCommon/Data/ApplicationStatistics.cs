using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Common.Data
{
    public static class ApplicationStatistics
    {
        public static int ADContextCount { get; private set; }
        public static void AddADContext()
        {
            ADContextCount++;
        }
        public static void RemoveADContext()
        {
            if (ADContextCount > 0)
                ADContextCount--;

        }


        public static int DBContextCount { get; private set; }
        public static void AddDBContext()
        {
            DBContextCount++;
        }
        public static void RemoveDBContext()
        {
            if (DBContextCount > 0)
                DBContextCount--;

        }



    }
}
