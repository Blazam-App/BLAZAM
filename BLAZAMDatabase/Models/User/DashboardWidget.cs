

namespace BLAZAM.Database.Models.User
{
    public enum Widget
    {
        NewUsers,
        LockedOutUsers,
        PasswordsChanged
    }
    internal class DashboardWidget : AppDbSetBase
    {
        public Widget WidgetID { get; set; }
        public int Slot { get; set; }
        public int Order { get; set; }
        public AppUser User { get; set; }

    }
}
