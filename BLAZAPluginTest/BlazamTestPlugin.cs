using BLAZAM.Global.Data;
using BLAZAM.Plugins;
using BLAZAM.TestPlugin.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace BLAZAPluginTest
{
    public class BlazamTestPlugin : IPluginBase, IPluginServiceProvider,IPluginDbContext,IPluginModelCreating
    {


        public string Name => "Example Plugin";

        public PluginVersion Version => new("1.0.0");

        public string Author => "Blazam";

        public static Guid Guid => Guid.Parse("e2a1c7b2-4f8e-4c3a-9b7e-2d8f1a6e5c3f");

        public Type DbContextType => typeof(TestPluginDbContext);

        public WebApplicationBuilder InjectServices(WebApplicationBuilder builder)
        {
            return builder;
        }

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExampleData>().HasData(
                new ExampleData { Id = 1, PluginId = Guid, Name = "Test 1", Description = "This is a test" },
                new ExampleData { Id = 2, PluginId = Guid, Name = "Test 2", Description = "This is another test" }
            );
        }
    }
}
