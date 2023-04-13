

namespace BLAZAM.Database.Models.User
{
    public enum DashboardWidgetType
    {
        NewUsers,
        LockedOutUsers,
        PasswordsChanged
    }
    internal class DashboardWidget : AppDbSetBase
    {
        public DashboardWidgetType WidgetType { get; set; }
        public int Slot { get; set; }
        public int Order { get; set; }
        public AppUser User { get; set; }

    }
}
