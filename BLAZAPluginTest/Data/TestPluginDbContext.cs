using BLAZAM.Global.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLAZAPluginTest.Data
{
    public class TestPluginDbContext : DbContext, ISqliteAppDbContext
    {
        public TestPluginDbContext()
        {
        }


        public TestPluginDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(
                           "Data Source=database.db").EnableSensitiveDataLogging();
        }

        public DbSet<ExampleData> ExampleDatas { get; set; }


    }
}
