# Configuration
All web host application settings are set in the `appsettings.json`
file in the root path of the application directory.
## AppSettings
`appsettings.json`
### Example File
```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.Hosting.Lifetime": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
    }
  },
  "DebugMode": "false",
  "InstallType": "IIS",
  "ListeningAddress": "*",
  "HTTPPort": "79",
  "HTTPSPort": "442",
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "SQLConnectionString": "Data Source=localhost;Database=BLAZAM;Persist Security Info=True;Integrated Security=False;Connection Timeout=10;TrustServerCertificate=True;"
  }

}
```
### Config Options
#### Logging
It is recommended not to modify these settings

#### DebugMode
| Values      | Description                          |
| ----------- | ------------------------------------ |
| `true`      | The application will provide a demo user for login  |
| `false`     | The application will operate in the normal mode |

#### InstallType

!!! note inline end

    This will likely not be implemented
| Values      | Description                          |
| ----------- | ------------------------------------ |
| `IIS`       | Lets the application know it is running under IIS |
| `Service`   | Lets the application know it is running as a service |


#### HTTPPort

!!! info

    This setting has no effect when running under IIS
| Values      | Description                          |
| ----------- | ------------------------------------ |
| `PortNumber`       | If running as a service, the application will listen for HTTP connections on this port|



#### HTTPSPort

!!! info

    This setting has no effect when running under IIS
| Values      | Description                          |
| ----------- | ------------------------------------ |
| `PortNumber`       | If running as a service, the application will listen for HTTPS connections on this port|


#### AllowedHosts

| Values      | Description                          |
| ----------- | ------------------------------------ |
| `*`         | Allows all IP addresses to communicate with the Blazam|
| `subnet/mask`| Allows only IP's from the defined subnet to communicate with the Blazam|


#### SQLConnectionString

| Values      | Description                          |
| ----------- | ------------------------------------ |
| `string`         | The connection string to connect to your SQL server. If you need a generator try [this one](https://www.aireforge.com/tools/sql-server-connection-string-generator).|
