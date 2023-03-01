#Name BLAZAM Updater Script
#Purpose Provides a decoupled way to elevate, and update web app
#Version 1.1.5



param(
    [string]$Username,
    [string]$Password,
    [string]$Domain,
    [string]$processId,
    [string]$UpdateSourcePath,
    [string]$ApplicationDirectory
)
Write-Host("Performs a self update of the BLAZAM Web Application.")

Write-Host("Usage:`r`n updater.ps1 -Username user -Password pass -ProcessId 1234 -UpdateSourcePath 'C:\UpdateSourcePath\_BLAZAM' -ApplicationDirectory 'C:\inetpub\blazam\'")
<#
Allow the script to run as the current user
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
#>
if ($processId -eq "") {
    Write-Host("Error: Process ID not provided")
    exit
}

Write-Host("Username: " + $Username);
Write-Host("Password: " + $Password);
Write-Host("Domain: " + $Domain);
Write-Host("ApplicationDirectory: " + $ApplicationDirectory);
Write-Host("Update Source Path: " + $UpdateSourcePath);
Write-Host("Process Id: " + $processId);



$updateScript= {
     param(
        [string]$sourceParam,
        [string]$destinationParam,
        [string]$processIdParam
    )
    function Quit {
        Stop-Transcript -ErrorAction SilentlyContinue
        exit
    }

    function StopApp {

        New-Item -Path $global:destination -Name "app_offline.htm" -ItemType "file"
        
        
        if(!$global:iis){
            Stop-Process -ID $global:processId -Force
        }
        
        Start-Sleep -Seconds 15
        
    }
    
    function StartApp {
       

            Remove-Item -Path $global:destination\app_offline.htm -Force
        
        if(!$global:iis){
            Write-Host("Process path: " + $global:processFilePath)

            if($global:processArguments -ne $null -and $global:processArguments -ne "")
            {
                
                Write-Host("Starting with arguments: " + $global:processArguments)
                $restartedProcess = Start-Process -FilePath $global:processFilePath -ArgumentList $global:processArguments -WorkingDirectory $global:destination -PassThru

            }else{
                $restartedProcess = Start-Process -FilePath $global:processFilePath -WorkingDirectory $global:destination -PassThru

            }
            Write-Host("Waiting 15 seconds for Application to restart")
            Start-Sleep -Seconds 15
            <#
            Write-Host("Process Stats: "+($restartedProcess| Select-Object *))
            if ($restartedProcess.ExitTime -eq $null -or $global:iis) {
                Write-Host("Error: Web Application failed to restart rolling back changes")
                #Perform Rollback Section
                $restoreSource = $backupDirectory + "*"
                Write-Host("Rolling back update")
                Write-Host("Source: " + $restoreSource)
                Write-Host("Destination: " + $global:destination)
                Copy-Item -Path $restoreSource -Destination $backupDirectory -Recurse -Verbose -Force
                Write-Host("Restarting ApplicationPool")
        
                if($global:processArguments -ne $null -and $global:processArguments -ne "")
                {
                    
                    Write-Host("Starting with arguments: " + $global:processArguments)
                    $restartedProcess = Start-Process -FilePath $global:processFilePath -ArgumentList $global:processArguments -PassThru
            
                }else{
                    $restartedProcess = Start-Process -FilePath $global:processFilePath -PassThru
            
                }
                #$runningSite | Start-IISSite
                Write-Host("Waiting 15 seconds for Application to restart")
            
                Start-Sleep -Seconds 15
                if ($restartedProcess.ExitTime -eq $null) {
                    Write-Host("Error: Rollback performed, but application did not start! Oh no...")
                }
                else {
                    Write-Host("Rollback completed successfully")
                }
                Stop-Transcript
                exit
            }
            #>
        }
        

    }

    function PerformBackup{
        #Perform Backup Section
        $date = Get-Date
        $backupDateStr = $date.ToString("yyyyMMddHHmmss")
        $backupDirectory = $env:TEMP + "\BLAZAM\backup\" + $backupDateStr + "\"

        Write-Host("Backup path: " + $backupDirectory)


        $backupSource = $global:destination + "*"
        Write-Host("Backing up current")
        Write-Host("Source: " + $backupSource)
        Write-Host("Destination: " + $backupDirectory)
        Copy-Item -Path $backupSource -Destination $backupDirectory -Recurse -Verbose -Force



        Start-Sleep -Seconds 2
    }

  function ApplyUpdate{
        #Apply Update Section
        $global:source = $global:source + "\*"
        Write-Host("Applying Update")
        Write-Host("Source: " + $global:source)
        Write-Host("Destination: " + $global:destination)
        Copy-Item -Path $global:source -Destination $global:destination  -Exclude "*\updater\*" -Recurse -Verbose -Force
        
        Start-Sleep -Seconds 2
    }


        $global:iis = $false
        $global:source = $sourceParam
        $global:destination = $destinationParam
        $global:processId = $processIdParam

Write-Host("Global Process Id: " +  $global:processId);

    $logPath = $global:destination + "updater\lastUpdateAttempt.txt"

    Write-Host("Log path: " + $logPath)
    Start-Transcript -Path $logPath


   

    if (($global:source -eq $null) -or ($global:source -eq "")) {
        Write-Host("Error: Source was not provided!")
        Write-Host("You must provide the update source path. This should be the unzipped application root directory")
        Quit
    }
    
    if (($global:destination -eq $null) -or ($global:destination -eq "")) {
        Write-Host("Error: Destination was not provided!")
        Write-Host("You must provide the current application path. This should be the current application root directory")

        Quit
    }

        if (($global:processId -eq $null) -or ($global:processId -eq "")) {
        Write-Host("Error: Process ID was not provided!")
        Write-Host("You must provide the running proceess ID. You cannot update the app while stopped.")

        Quit
    }
    
    Write-Host("Update Source Path: " + $global:source);
    Write-Host("Destination Path: " + $global:destination);
    Write-Host("Process ID: " + $global:processId);
    Write-Host("Running as " + $env:UserDomain + "\" + $env:UserName);
    
    if (!(Test-Path -Path $global:destination -PathType Container)) {
        Write-Host("Error: Destination directory doesn't exist. Quitting.")
        Quit
        }

    $process = Get-Process -Id $global:processId
    Write-Host("Process Stats: "+($process| Select-Object *))

    if($process -eq $null){
        Write-Host("Warning: A process with process id of "+$global:processId+" was not found")
        
    }
    elseif($process.Name -eq "w3wp"){
        $global:iis = $true
    }

    if(!$global:iis){
        $commandLine = (gcim win32_process | Where-Object -Property "processid" -EQ -Value $global:processId | Select-Object commandline).commandline

        Write-Host("Command Line for running: " +$commandLine)
        $global:processFilePath = ($commandLine -split " ")[0]

            Write-Host("Process path running: " +$global:processFilePath)
        if($global:processFilePath.Contains("w3wp")){
            
            Write-Host("Is IIS")
            $hParam = ($commandLine | Select-String -Pattern '-h ".*?"' -AllMatches).Matches.Value -replace '-h ','' -replace '"',''
            $global:processArguments ='-h "'+$hParam +'"'
        }

        Write-Host("IIS: "+$global:iis)
        Write-Host "Re-Launch Command "$global:processFilePath" "$global:processArguments
    }
   

    StopApp
    if($process.ExitTime -ne $null)
    {
        
        Write-Host("Error: Web Application failed to stop")
        Quit
    }
    
    PerformBackup
   
   ApplyUpdate

    Write-Host("Restarting Application")
    
    StartApp
    Write-Host("Is IIS: " + $global:iis)
    
    Write-Host("Web Application successfully updated")
    Stop-Transcript
}

if($Username -ne "" -and $Password -ne "")
{
$securePassword = ConvertTo-SecureString $Password -AsPlainText -Force
$credential = New-Object System.Management.Automation.PSCredential($Username, $securePassword)
$out = Invoke-Command -ComputerName "localhost"  -Credential $credential -ArgumentList  $UpdateSourcePath, $ApplicationDirectory,$processId -EnableNetworkAccess -ScriptBlock $updateScript

}
else{
$out = Invoke-Command -ArgumentList  $UpdateSourcePath, $ApplicationDirectory,$processId  -ScriptBlock $updateScript

}
Write-Host($out)
exit


