

using BLAZAM.Database.Models;

namespace BLAZAM.TestPlugin.Data
{
    public class ExampleData : PluginDbSetBase
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
