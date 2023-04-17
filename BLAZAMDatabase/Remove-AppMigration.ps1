param(
    [string]$Name
)

$sqlName =  $Name + "Sql";
$mysqlName =  $Name + "MySql";
$sqliteName =  $Name + "Sqlite";
try {
    Remove-Migration -Context SqliteDatabaseContext -Force
} catch {
    exit
}

try {
    Remove-Migration -Context SqlDatabaseContext -Force
} catch {
    exit
}

try {
    Remove-Migration -Context MySqlDatabaseContext -Force
    
} catch {
    exit
}



