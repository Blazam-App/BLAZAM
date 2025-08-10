using BLAZAM.Database.Exceptions;
using BLAZAM.FileSystem;

namespace BLAZAM.Common.Data.Database
{
    public enum DatabaseType { SQL, MySQL, SQLite }
    /// <summary>
    /// Represents a .NET ConnectionString
    /// </summary>
    public class DatabaseConnectionString
    {

        public DatabaseConnectionString(string connectionString)
        {
            Value = connectionString;
        }
        public DatabaseConnectionString(string? connectionString, DatabaseType dbType)
        {

            Value = connectionString;
            DatabaseType = dbType;
        }

        /// <summary>
        /// The type of database calculated from this ConnectionString
        /// </summary>
        public DatabaseType DatabaseType { get; set; }

        /// <summary>
        /// Returns true if the <see cref="ServerAddress"/> ends with ".db"
        /// </summary>
        public bool FileBased => ServerAddress.EndsWith(".db");

        /// <summary>
        /// Returns a file that points to the <see cref="ServerAddress"/>.
        /// This should only be used for SQLite.
        /// </summary>
        public SystemFile File => new(AddressComponent);

        /// <summary>
        /// The full ConnectionString to the database
        /// </summary>
        public string? Value { get; set; }
        protected string AddressComponent
        {
            get
            {
                if (Value != null)
                {
                    string search = "data source=";
                    int startIndex = Value.ToLower().IndexOf(search);
                    if (startIndex == -1)
                    {
                        search = "server=";
                        startIndex = Value.ToLower().IndexOf(search);
                    }
                    if (startIndex >= 0)
                    {
                        startIndex += search.Length;
                        int endIndex = Value.IndexOf(";", startIndex);
                        if (endIndex >= 0)
                        {
                            return Value.Substring(startIndex, endIndex - startIndex);

                        }
                        if (endIndex == -1)
                        {
                            return Value.Substring(startIndex);

                        }

                    }

                }
                throw new AppException("Connection String missing a Server or Data Source parameter");
            }
        }
        /// <summary>
        /// Returns the database connected to based on the full ConnectionString
        /// </summary>
        public string Database
        {
            get
            {
                if (Value == null)
                    throw new AppException("Connection String missing a Database or Initial Catalog parameter");

                if (FileBased)
                    return "File Based";

                string[] keys = { "initial catalog=", "database=" };
                string lowerValue = Value.ToLower();

                foreach (var key in keys)
                {
                    int startIndex = lowerValue.IndexOf(key);
                    if (startIndex >= 0)
                    {
                        startIndex += key.Length;
                        int endIndex = Value.IndexOf(";", startIndex);
                        return endIndex >= 0
                            ? Value.Substring(startIndex, endIndex - startIndex)
                            : Value.Substring(startIndex);
                    }
                }

                throw new AppException("Connection String missing a Database or Initial Catalog parameter");
            }
        }
        /// <summary>
        /// The server IP or hostname as defined in the ConnectionString
        /// </summary>
        public string ServerAddress
        {
            get
            {

                var fullAddress = AddressComponent;

                if (fullAddress != null)
                {
                    string[] dataSourceParts = fullAddress.Split(',');
                    if (dataSourceParts.Length > 0)
                    {
                        string serverFragment = dataSourceParts[0];
                        if (serverFragment.StartsWith("tcp:"))
                            serverFragment = serverFragment.Substring(4);
                        if (serverFragment.Contains("\\") && !serverFragment.Contains(":\\"))
                        {
                            serverFragment = serverFragment.Split("\\")[0];
                        }
                        return serverFragment;  // Outputs "serverNameOrIp"
                    }






                }
                throw new DatabaseConnectionStringException("Error getting server address from appconfig");

            }


        }
        /// <summary>
        /// The server IP or hostname as defined in the ConnectionString
        /// </summary>
        public string? InstanceName
        {
            get
            {

                var fullAddress = AddressComponent;

                if (fullAddress != null)
                {
                    string[] dataSourceParts = fullAddress.Split(',');
                    if (dataSourceParts.Length > 0)
                    {
                        string serverFragment = dataSourceParts[0];

                        if (serverFragment.Contains("\\"))
                        {
                            return serverFragment.Split("\\")[1];
                        }
                    }


                    return null;




                }
                throw new DatabaseConnectionStringException("Error getting server address from appconfig");

            }


        }

        /// <summary>
        /// The database server port as defined in the ConnectionString
        /// </summary>
        public int ServerPort
        {
            get
            {

                var fullAddress = AddressComponent;

                if (fullAddress != null)
                {
                    string[] dataSourceParts = fullAddress.Split(',');
                    if (dataSourceParts.Length == 2)
                    {
                        string portFragment = dataSourceParts[1];
                        return int.Parse(portFragment);  // Outputs "serverPort"
                    }
                    else if (dataSourceParts.Length == 1)
                    {
                        switch (DatabaseType)
                        {
                            case DatabaseType.SQL:
                                return 1433;
                            case DatabaseType.MySQL:
                                return 3306;
                        }
                        return 0;
                    }
                    else
                    {
                        throw new AppException("The server string has too many components in AppSettings");
                    }






                }
                throw new DatabaseConnectionStringException("Error getting server port from appconfig");

            }


        }
    }
}
