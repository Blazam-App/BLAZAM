using BLAZAM.Common.Data.Database;

namespace BLAZAMDatabaseTests
{
    public class UnitTest1
    {
        const string Valid_SQLite_Connection_String = "Data Source=C:\\ProgramData\\Blazam\\database.db;";
        const string Valid_SQL_Connection_String = "Data Source=sql-blazam-org;Database=BlazamTest;User Id=sa;Password=blazam;";
        const string Valid_SQLExpress_Connection_String = "Data Source=sql-blazam-org\\SQLEXPRESS,1433;Database=BlazamTest;User Id=sa;Password=blazam;";
        [Fact]
        public void Valid_SQLite_Returns_Valid_File()
        {
            var cstring = new DatabaseConnectionString(Valid_SQLite_Connection_String);

            Assert.True(cstring.File.FullPath == Valid_SQLite_Connection_String.Replace(";","").Split("=")[1]);
        }
        [Fact]
        public void Valid_SQL_Returns_Valid_Data()
        {
            var cstring = new DatabaseConnectionString(Valid_SQL_Connection_String);

            Assert.True(cstring.ServerAddress=="sql-blazam-org");
            Assert.True(cstring.Database=="BlazamTest");          
        }

        [Fact]
        public void Valid_SQLExpress_Returns_Valid_Data()
        {
            var cstring = new DatabaseConnectionString(Valid_SQLExpress_Connection_String);

            Assert.True(cstring.ServerAddress == "sql-blazam-org");
            Assert.True(cstring.InstanceName == "SQLEXPRESS");
            Assert.True(cstring.ServerPort == 1433);
            Assert.True(cstring.Database == "BlazamTest");
        }
    }
}