using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Common.Models.Database.User
{
    public enum Widget
    {
        NewUsers,
        LockedOutUsers,
        PasswordsChanged
    }
    internal class DashboardWidget
    {
        public int Id { get; set; }
        public Widget WidgetID { get; set; }
        public int Slot { get; set; }
        public int Order { get; set; }

    }
}
