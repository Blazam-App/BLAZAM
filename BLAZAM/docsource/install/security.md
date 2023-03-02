# Security

Blazam adheres to a strict delegation of elevated privileges. It is designed to run under an un-privileged user account.


!!! abstract

    The developers of Blazam always keep security and priviledge protection as a top priority. 
    
    All password are encrypted both at rest and in transit. All incoming/outgoing connections are TLS/SSL capable.  
    
    Having said that, we take no responsibillity
    for any damages incurred from your use of this software. You are encouraged to review the source code for yourself.

!!! danger

    Running the web application under elevated priviledges exposes your Active Directory to unneccessary risk of framework exploits.

The application only has as much priviledge as you supply it. It is possible to set up an advanced permission ACL within Active
Directory for the user account provided for AD communication to limit the exposure of the application.
## Application User

!!! danger
    
    Do not run the IIS application pool or application service as an adminstrator or System account.
### For IIS
Use the default IIS_User account provided to the application pool.
### For Service
Using the NetworkService account is reccomended.
## Folder Permissions
For most deployments, no modifications to folder permissions are required.

The following conditions warrant changing application root directory permissions:
   
* The user account used for application updates is not already an administrator of the web server.
* You want to configure a separate account to run self-updates under, if that account is not a local
administrator.

## Encryption

!!! note

    We are considering moving the private key from the `%temp%\Blazam` directory to the appsettings.json

The application encrypts sensitive database data such as passwords. Blazam automatically creates a random
private key to use for this encryption on first launch.

### Private Key
The privatte key in the current version is stored under `%temp%\Blazam\writable\security\database.kety`

The temp directory is the temp directory of the running user of the application, or `C:\Windows\Temp`

!!! tip "Backup the Private Key"

    It is highly recommended to backup the private key immediatly following the installation wizard.
    
    Loss of the private key will result in the inabillity to log in as the application `admin`,, and
    break communication with your Actvie Directory, effectivley locking you out without manual modifications
    to the database.

    To backup the key, go to the `Settings` page and click the `System` tab.