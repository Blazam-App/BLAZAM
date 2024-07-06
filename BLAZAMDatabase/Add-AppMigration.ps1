param(
    [string]$Name
)

$sqlName =  $Name + "Sql";
$mysqlName =  $Name + "MySql";
$sqliteName =  $Name + "Sqlite";
try {
    Add-Migration -Name $sqliteName -OutputDir Migrations/Sqlite -Context SqliteDatabaseContext
} catch {
    exit
}

try {
    Add-Migration -Name $sqlName -OutputDir Migrations/Sql -Context SqlDatabaseContext
} catch {
    Remove-Migration -Context SqliteDatabaseContext
    exit
}

try {
    Add-Migration -Name $mysqlName -OutputDir Migrations/MySql -Context MySqlDatabaseContext
    
} catch {
    Remove-Migration -Context SqliteDatabaseContext
    Remove-Migration -Context SqlDatabaseContext
    exit
}



