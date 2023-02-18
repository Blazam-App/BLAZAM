using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Common.Data
{
    public class FileProgress
    {
        public int FilePercentage { get => (int)((double)CompletedBytes / ExpectedSize * 100); }
        public int ExpectedSize { get; set; }
        public int CompletedBytes { get; set; }
    }
}
