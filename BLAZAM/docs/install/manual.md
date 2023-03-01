## Pre-Requisites
* Install an SQL Express or MS SQL server, or use an existing one.
* Create an empty database, a new SQL or AD user to connect with, and permissions and logons set.
	* Refer to your database documentation for security setup and best practices
* Download and [Install .NET Core 6.x Runtime](https://aka.ms/dotnet-download) from Micrsoft
	* If running under IIS, you will also need the [.NET Core 6.x Web Hosting Bundle](https://aka.ms/dotnet-download), also from Microsft.
### [Download](https://blazam.org/download)

## Install under IIS
Feel free to deviate from the instructions to fit your desired deployment

1. Install the Application Initialization Module
	* You can find the module under Server Roles -> Web Server -> Application Developer -> Application Initialization.
1. Create new Application Pool in IIS
1. Set Pool to AlwayRunning
	* In IIS Manager, right click on the application pool under which the application runs and select “Advanced Settings”.	
    * Set start mode to “Always Running”.
1. Create new Site in IIS for Blazam
1. Set IIS Site to Preload `(Optional)`
	* In IIS Manager, right click on the site for the application, select “Manage Website” -> “Advanced Settings” and set the “Preload Enabled” value to true.
1. Copy contents of zip file to site directory
1. Copy appsettings.json.example and rename to appsettings.json
1. Replace SQLConnectionString value with the connection string to your database

## Install as Service