using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Database.Models
{
    public class GenericSidList : AppDbSetBase
    {
        public string Sid { get; set; }
        public DateTime Added { get; set; }
    }
}
