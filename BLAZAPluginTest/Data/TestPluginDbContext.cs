using Microsoft.EntityFrameworkCore;

namespace BLAZAM.TestPlugin.Data
{
    public class TestPluginDbContext : DbContext
    {
        public DbSet<ExampleData> ExampleDatas { get; set; }

        public TestPluginDbContext(DbContextOptions<TestPluginDbContext> options) : base(options)
        {
        }
    }
}
