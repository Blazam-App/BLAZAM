#Name BLAZAM Updater Script
#Purpose Provides a decoupled way to elevate, and update web app
#Version 1.1.0



param(
    [string]$Username,
    [string]$Password,
    [string]$Domain,
    [string]$ProcessId,
    [string]$UpdateSourcePath,
    [string]$ApplicationDirectory
)
Write-Host("Performs a self update of the BLAZAM IIS Web Application.")

Write-Host("Usage:`r`n updater.ps1 -Username user -Password pass -ProcessId 1234 -UpdateSourcePath 'C:\UpdateSourcePath\_BLAZAM' -ApplicationDirectory 'C:\inetpub\blazam\'")

if ($Username -eq "") {
    Write-Host("Error: Username not provided")
    exit
}
if ($Password -eq "") {
    Write-Host("Error: Password not provided")
    exit
}
if ($Domain -eq "") {
    Write-Host("Error: Domain not provided")
    exit
}

if ($ProcessId -eq "") {
    Write-Host("Error: Process ID not provided")
    exit
}

Write-Host("Username: " + $Username);
Write-Host("Password: " + $Password);
Write-Host("Domain: " + $Domain);
Write-Host("ApplicationDirectory: " + $ApplicationDirectory);
Write-Host("Update Source Path: " + $UpdateSourcePath);


$securePassword = ConvertTo-SecureString $Password -AsPlainText -Force
$credential = New-Object System.Management.Automation.PSCredential($Username, $securePassword)

$out = Invoke-Command -ComputerName "localhost" -Credential $credential -ArgumentList  $UpdateSourcePath, $ApplicationDirectory,$procProcessIdessId  -ScriptBlock {
    param(
        [string]$source,
        [string]$destination,
        [string]$processId
    )
    function Quit {
        Stop-Transcript -ErrorAction SilentlyContinue
        exit
    }

    function Get-AppPool{
        $sites = Get-IISSite;

        foreach ($site in $sites) {
    
            $bindings = Get-IISSiteBinding -Name $site.Name
            #Write-Host ($site);
    
            # Write-Host ($binding);
            foreach ($binding in $bindings) {
            
                #Write-Host ($binding.Host);
                if ($binding.Host -contains $address) {
                    #Write-Host ("Found Site")
                    $runningSite = $site
                    #Write-Host("SiteName: "+$site.Name)
                    $pools = Get-IISAppPool
                    foreach ($pool in $pools) {
                        #Write-Host("Pool Name: "+$pool.Name)
                        #Write-Host("Applications: "+$site.Applications)
                        [string] $testPoolName = $pool.Name + "/"
                        [string] $appName = $site.Applications
                        #Write-Host("Testing: "+$testPoolName + "==" + $appName)
                        if ($appName -eq $testPoolName) {
                            #Write-Host("Found app pool")
                            $runningAppPool = $pool
                            break
                        }
                    }
                }
            }
        }
        return $runningAppPool
    }

    $date = Get-Date
    $DateStr = $date.ToString("yyyyMMddHHmmss")
    $backupDirectory = $env:TEMP + "\BLAZAM\backup\" + $DateStr + "\"

    $logPath = $destination + "updater\testlog.txt"

    Write-Host("Log path: " + $logPath)
    Start-Transcript -Path $logPath
    Write-Host("Backup path: " + $backupDirectory)

    if (($source -eq $null) -or ($source -eq "")) {
        Write-Host("Error: Source was not provided!")
        Quit
    }
    
    if (($destination -eq $null) -or ($destination -eq "")) {
        Write-Host("Error: Destination was not provided!")
        Quit
    }

        if (($processId -eq $null) -or ($processId -eq "")) {
        Write-Host("Error: Process ID was not provided!")
        Quit
    }
    
    Write-Host("Update Source Path: " + $source);
    Write-Host("Destination Path: " + $destination);
    Write-Host("Process ID: " + $processId);

    $runningAppPool
    $runningSite


    Write-Host("Running as " + $env:UserDomain + "\" + $env:UserName);
    
<#
    $sites = Get-IISSite;

    foreach ($site in $sites) {

        $bindings = Get-IISSiteBinding -Name $site.Name
        Write-Host ($site);

        # Write-Host ($binding);
        foreach ($binding in $bindings) {
        
            Write-Host ($binding.Host);
            if ($binding.Host -contains $address) {
                #Write-Host ("Found Site")
                $runningSite = $site
                #Write-Host("SiteName: "+$site.Name)
                $pools = Get-IISAppPool
                foreach ($pool in $pools) {
                    #Write-Host("Pool Name: "+$pool.Name)
                    #Write-Host("Applications: "+$site.Applications)
                    [string] $testPoolName = $pool.Name + "/"
                    [string] $appName = $site.Applications
                    #Write-Host("Testing: "+$testPoolName + "==" + $appName)
                    if ($appName -eq $testPoolName) {
                        Write-Host("Found app pool")
                        $runningAppPool = $pool
                        break
                    }
                }
            }
        }
    }
    if ($runningAppPool -eq $null) {
        Write-Host("Error: Site Not Found: " + $address)
        Quit
    }

    Write-Host("App Pool Name: " + $runningAppPool.Name)
    Write-Host("Site State: " + $runningSite.State)
        
    Write-Host("Pool State: " + $runningAppPool.State)
    
#    if ($runningSite.State -ne 'Stopped') {
#        
#        $runningSite | Stop-IISSite -Confirm:$false
#    }
#    Write-Host("Waiting up to 60 seconds for Application to stop")
#    $retries = 60
#    while ($retries -gt 0) {
#        Start-Sleep -Seconds 1
#        $retries = $retries - 1
#        if ($runningSite.State -eq 'Stopped') {
#            Write-Host("Application stopped")
#            $retries = 0
#        }else{
#            Write-Host("Application not stopped yet.")
#        }
#    }
#    if ($runningSite.State -ne 'Stopped') {
#        
#        Write-Host("Error: Web Application failed to shutdown")
#        Quit
#    }

    if ($runningAppPool.State -ne 'Stopped') {
        
        $runningAppPool | Stop-WebAppPool
    }

    Write-Host("Waiting up to 60 seconds for ApplicationPool to stop")
    $retries = 60
    while ($retries -gt 0) {
        Start-Sleep -Seconds 1
        $retries = $retries - 1
        if ($runningAppPool.State -eq 'Stopped') {
            Write-Host("Application Pool stopped")
            $retries = 0
        }else{
            
            Write-Host($runningAppPool.Name + ":"+$runningAppPool.State)
            Write-Host("Application Pool not stopped yet.")
        }
    }
    Write-Host("Site State: " + $runningSite.State)
        
    Write-Host("Pool State: " + $runningAppPool.State)
        
    if ($runningAppPool.State -ne 'Stopped') {
        Write-Host("Error: Web Application Pool failed to shutdown")
        Quit

    }
#>
    $process = Get-Process -Id $processId
    $commandLine = gcim win32_process | Where-Object -Property "processid" -EQ -Value $processId | Select-Object commandline

    $processFilePath = ($commandLine -split " ")[0]
    if($processFilePath.Contains("w3wp.exe")){
        $hParam = ($commandLine | Select-String -Pattern '-h ".*?"' -AllMatches).Matches.Value -replace '-h ','' -replace '"',''
        $relaunchCommand ='-h "'+$hParam +'"'
    }
    else{
        $relaunchCommand = $commandLine
    }
    Write-Host "Re-Launch Command" $relaunchCommand

    Stop-Process -ID $processId -Force
    
    Start-Sleep -Seconds 2
    if($process.ExitTime -ne $null)
    {
        
        Write-Host("Error: Web Application failed to stop")
        Quit
    }
    #Perform Backup Section
    $backupSource = $destination + "*"
    Write-Host("Backing up current")
    Write-Host("Source: " + $backupSource)
    Write-Host("Destination: " + $backupDirectory)
    Copy-Item -Path $backupSource -Destination $backupDirectory -Recurse -Verbose -Force



    Start-Sleep -Seconds 2
    #Apply Update Section
    $source = $source + "\*"
    Write-Host("Applying Update")
    Write-Host("Source: " + $source)
    Write-Host("Destination: " + $destination)
    Copy-Item -Path $source -Destination $destination  -Exclude "*\updater\*" -Recurse -Verbose -Force
    
    Start-Sleep -Seconds 2
    Write-Host("Restarting ApplicationPool")
    # $runningAppPool | Start-WebAppPool
    #  $runningSite | Start-IISSite
    
    Write-Host("Waiting 15 seconds for Application to restart")

    Start-Sleep -Seconds 15
    if ($runningSite.State -ne 'Started' -and $runningAppPool.State -ne 'Started') {
        Write-Host("Error: Web Application failed to restart rolling back changes")
        #Perform Rollback Section
        $restoreSource = $backupDirectory + "*"
        Write-Host("Rolling back update")
        Write-Host("Source: " + $restoreSource)
        Write-Host("Destination: " + $destination)
        Copy-Item -Path $restoreSource -Destination $backupDirectory -Recurse -Verbose -Force
        Write-Host("Restarting ApplicationPool")

        $runningAppPool | Start-WebAppPool
        #$runningSite | Start-IISSite
        Write-Host("Waiting 15 seconds for Application to restart")
    
        Start-Sleep -Seconds 15
        if ($runningSite.State -ne 'Started' -and $runningAppPool.State -ne 'Started') {
            Write-Host("Error: Rollback performed, but application did not start! Oh no...")
        }
        else {
            Write-Host("Rollback completed successfully")
        }
        Stop-Transcript
        exit
    }
    Write-Host("Web Application successfully updated")
    Stop-Transcript
}
Write-Host($out)
exit


