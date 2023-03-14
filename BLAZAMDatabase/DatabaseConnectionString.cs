using Microsoft.VisualStudio.Services.Zeus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Database.Data.Database
{
    public enum DatabaseType { SQL, MySQL, SQLite }
    public class DatabaseConnectionString
    {
        public DatabaseConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
        }
        public DatabaseConnectionString(string? connectionString, DatabaseType dbType)
        {

            ConnectionString = connectionString;
            //ConnectionString = ConnectionString.Replace("%temp%", Path.GetTempPath().Substring(0, Path.GetTempPath().Length-1));
            DatabaseType = dbType;
        }
        public DatabaseType DatabaseType;
        public bool FileBased => ServerAddress.EndsWith(".db");
        public SystemFile File => new(ServerAddress);
        public string? ConnectionString { get; set; }
        public string AddressComponent
        {
            get
            {
                if (ConnectionString != null)
                {
                    string search = "Data Source=";
                    int startIndex = ConnectionString.IndexOf(search);
                    if (startIndex == -1)
                    {
                        search = "Server=";
                        startIndex = ConnectionString.IndexOf(search);
                    }
                    if (startIndex >= 0)
                    {
                        startIndex += search.Length;
                        int endIndex = ConnectionString.IndexOf(";", startIndex);
                        if (endIndex >= 0)
                        {
                            return ConnectionString.Substring(startIndex, endIndex - startIndex);

                        }

                    }

                }
                throw new ApplicationException("Connection String missing a Server or Data Source parameter");
            }
        }
        public string Database
        {
            get
            {
                if (ConnectionString != null)
                {
                    if (FileBased) return "File Based";

                    string search = "Initial Catalog=";
                    int startIndex = ConnectionString.IndexOf(search);
                    if (startIndex == -1)
                    {
                        try
                        {
                            search = "Database=";
                            startIndex = ConnectionString.IndexOf(search);
                        }
                        catch
                        {

                        }
                    }
                    if (startIndex >= 0)
                    {
                        startIndex += search.Length;
                        int endIndex = ConnectionString.IndexOf(";", startIndex);
                        if (endIndex >= 0)
                        {
                            return ConnectionString.Substring(startIndex, endIndex - startIndex);

                        }

                    }

                }
                throw new ApplicationException("Connection String missing a Database or Initial Catalog parameter");
            }
        }

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
                        return serverFragment;  // Outputs "serverNameOrIp"
                    }






                }
                throw new DatabaseConnectionStringException("Error getting server address from appconfig");

            }


        }

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
                        throw new ApplicationException("The server string has too many components in AppSettings");
                    }






                }
                throw new DatabaseConnectionStringException("Error getting server port from appconfig");

            }


        }
    }
}
