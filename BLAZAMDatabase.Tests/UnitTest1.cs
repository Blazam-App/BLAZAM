using BLAZAM.Common.Data.Database;
using SQLitePCL;

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
            var cstring = new DatabaseConnectionString(raw,DatabaseType.SQLite);

            Assert.True(cstring.File.FullPath == raw.Replace(";","").Split("=")[1]);
        }
        [Theory]
        [InlineData(Valid_SQL_Connection_String)]
        [InlineData(Valid_SQL_Connection_String2)]
        [InlineData(Valid_SQL_Connection_String3)]
        public void Valid_SQL_Returns_Valid_Data(string raw)
        {
            var cstring = new DatabaseConnectionString(raw, DatabaseType.SQL);

            Assert.True(cstring.ServerAddress=="sql-blazam-org");
            Assert.True(cstring.Database=="BlazamTest");          
        }

        [Theory]
        [InlineData(Valid_SQLExpress_Connection_String)]
        [InlineData(Valid_SQLExpress_Connection_String2)]
        [InlineData(Valid_SQLExpress_Connection_String3)]
        public void Valid_SQLExpress_Returns_Valid_Data(string raw)
        {
            var cstring = new DatabaseConnectionString(raw, DatabaseType.SQL);

            Assert.True(cstring.ServerAddress == "sql-blazam-org");
            Assert.True(cstring.InstanceName == "SQLEXPRESS");
            Assert.True(cstring.ServerPort == 1433);
            Assert.True(cstring.Database == "BlazamTest");
        }
    }
}