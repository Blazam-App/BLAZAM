using BLAZAM.Common.Data.Database;
using Xunit;
namespace BLAZAMDatabase.Tests
{
    public class UnitTest1
    {
        const string Valid_SQLite_Connection_String = "Data Source=C:\\ProgramData\\Blazam\\database.db;";
        const string Valid_SQLite_Connection_String2 = "data source=C:\\ProgramData\\Blazam\\database.db;";
        const string Valid_SQLite_Connection_String3 = "Data Source=C:\\ProgramData\\Blazam\\database.db";
        const string Valid_SQL_Connection_String = "Data Source=sql-blazam-org;Database=BlazamTest;User Id=sa;Password=blazam;";
        const string Valid_SQL_Connection_String2 = "data source=sql-blazam-org;database=BlazamTest;user id=sa;password=blazam;";
        const string Valid_SQL_Connection_String3 = "Data Source=sql-blazam-org;Database=BlazamTest;User Id=sa;Password=blazam";
        const string Valid_SQLExpress_Connection_String = "Data Source=sql-blazam-org\\SQLEXPRESS,1433;Database=BlazamTest;User Id=sa;Password=blazam;";
        const string Valid_SQLExpress_Connection_String2 = "data source=sql-blazam-org\\SQLEXPRESS,1433;database=BlazamTest;user id=sa;password=blazam;";
        const string Valid_SQLExpress_Connection_String3 = "Data Source=sql-blazam-org\\SQLEXPRESS,1433;Database=BlazamTest;User Id=sa;Password=blazam";

        [Theory]
        [InlineData(Valid_SQLite_Connection_String)]
        [InlineData(Valid_SQLite_Connection_String2)]
        [InlineData(Valid_SQLite_Connection_String3)]
        public void Valid_SQLite_Returns_Valid_File(string raw)
        {
            var cstring = new DatabaseConnectionString(raw, DatabaseType.SQLite);

            // Test File.FullPath property
            Assert.Equal(raw.Replace(";", "").Split("=")[1], cstring.File.FullPath);

            // Test FileBased property
            Assert.True(cstring.FileBased);

            // Test Database property
            Assert.Equal("File Based", cstring.Database);
        }

        [Theory]
        [InlineData(Valid_SQL_Connection_String)]
        [InlineData(Valid_SQL_Connection_String2)]
        [InlineData(Valid_SQL_Connection_String3)]
        public void Valid_SQL_Returns_Valid_Data(string raw)
        {
            var cstring = new DatabaseConnectionString(raw, DatabaseType.SQL);

            // Test ServerAddress property
            Assert.Equal("sql-blazam-org", cstring.ServerAddress);

            // Test Database property
            Assert.Equal("BlazamTest", cstring.Database);

            // Test FileBased property
            Assert.False(cstring.FileBased);

            // Test InstanceName property (should be null for non-SQLExpress)
            Assert.Null(cstring.InstanceName);

            // Test ServerPort property (should be default 1433 for SQL)
            Assert.Equal(1433, cstring.ServerPort);
        }

        [Theory]
        [InlineData(Valid_SQLExpress_Connection_String)]
        [InlineData(Valid_SQLExpress_Connection_String2)]
        [InlineData(Valid_SQLExpress_Connection_String3)]
        public void Valid_SQLExpress_Returns_Valid_Data(string raw)
        {
            var cstring = new DatabaseConnectionString(raw, DatabaseType.SQL);

            // Test ServerAddress property
            Assert.Equal("sql-blazam-org", cstring.ServerAddress);

            // Test InstanceName property
            Assert.Equal("SQLEXPRESS", cstring.InstanceName);

            // Test ServerPort property
            Assert.Equal(1433, cstring.ServerPort);

            // Test Database property
            Assert.Equal("BlazamTest", cstring.Database);

            // Test FileBased property
            Assert.False(cstring.FileBased);
        }
    }
}