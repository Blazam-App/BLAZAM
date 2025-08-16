namespace BLAZAM.Database.Models
{
    public class PluginSettings : AppDbSetBase
    {
        public string PluginName { get; set; } = string.Empty;
        public Guid PluginGuid { get; set; }
        public string? SettingsJson { get; set; } = string.Empty;

        /// <summary>
        /// Gets the settings for the plugin as a strongly typed object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSettings<T>() where T : class, new()
        {
            if (string.IsNullOrEmpty(SettingsJson))
            {
                return new T();
            }
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<T>(SettingsJson) ?? new T();
            }
            catch
            {
                return new T();
            }
        }
    }
}
