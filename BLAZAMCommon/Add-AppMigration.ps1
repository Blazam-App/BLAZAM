param(
    [string]$Name
)

$sqlName =  $Name + "Sql";
$mysqlName =  $Name + "MySql";
$sqliteName =  $Name + "Sqlite";
Add-Migration $sqliteName -OutputDir Migrations/Sqlite -Context SqliteDatabaseContext
Add-Migration $sqlName -OutputDir Migrations/Sql -Context SqlDatabaseContext
Add-Migration $mysqlName -OutputDir Migrations/MySql -Context MySqlDatabaseContext