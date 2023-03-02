using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Common.Data.Database
{
    public class DatabaseConnectionString
    {
        public DatabaseConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; set; }
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
                throw new ApplicationException("Error getting server address from appconfig");

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
                    return 1433;





                }
                throw new ApplicationException("Error getting server address from appconfig");

            }


        }
    }
}
