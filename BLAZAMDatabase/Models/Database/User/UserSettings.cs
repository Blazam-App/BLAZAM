namespace BLAZAM.Database.Models.Database.User
{
    public class UserSettings
    {
        public int UserSettingsId { get; set; }
        public string UserGUID { get; set; }
        public string? APIToken { get; set; }
        public string? Theme { get; set; }
        public bool SearchDisabledUsers { get; set; }
        public bool SearchDisabledComputers { get; set; }
        public string? Username { get; set; }
    }
    
}
