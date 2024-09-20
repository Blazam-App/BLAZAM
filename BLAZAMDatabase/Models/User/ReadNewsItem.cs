using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Database.Models.User
{
    public class ReadNewsItem : AppDbSetBase
    {
        public AppUser User { get; set; }
        public double NewsItemId { get; set; }
        public DateTime NewsItemUpdatedAt { get; set; }
    }
}
