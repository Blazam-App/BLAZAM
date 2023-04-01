param(
    [string]$Name
)

$sqlName =  $Name + "Sql";
$mysqlName =  $Name + "MySql";
$sqliteName =  $Name + "Sqlite";
try {
    Add-Migration $sqliteName -OutputDir Migrations/Sqlite -Context SqliteDatabaseContext
} catch {
    exit
}

try {
    Add-Migration $sqlName -OutputDir Migrations/Sql -Context SqlDatabaseContext
} catch {
    Remove-Migration -Context SqliteDatabaseContext
    exit
}

try {
    Add-Migration $mysqlName -OutputDir Migrations/MySql -Context MySqlDatabaseContext
    
} catch {
    Remove-Migration -Context SqliteDatabaseContext
    Remove-Migration -Context SqlDatabaseContext
    exit
}



