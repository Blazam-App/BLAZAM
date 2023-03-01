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
if ($ProcessId -eq "") {
    Write-Host("Error: Process ID not provided")
    exit
}

Write-Host("Username: " + $Username);
Write-Host("Password: " + $Password);
Write-Host("Domain: " + $Domain);
Write-Host("ApplicationDirectory: " + $ApplicationDirectory);
Write-Host("Update Source Path: " + $UpdateSourcePath);
Write-Host("Process Id: " + $ProcessId);



$updateScript= {
     param(
        [string]$source,
        [string]$destination,
        [string]$processId
    )
    function Quit {
        Stop-Transcript -ErrorAction SilentlyContinue
        exit
    }

    $logPath = $destination + "updater\lastUpdateAttempt.txt"

    Write-Host("Log path: " + $logPath)
    Start-Transcript -Path $logPath


    $date = Get-Date
    $backupDateStr = $date.ToString("yyyyMMddHHmmss")
    $backupDirectory = $env:TEMP + "\BLAZAM\backup\" + $backupDateStr + "\"

    Write-Host("Backup path: " + $backupDirectory)

    if (($source -eq $null) -or ($source -eq "")) {
        Write-Host("Error: Source was not provided!")
        Write-Host("You must provide the update source path. This should be the unzipped application root directory")
        Quit
    }
    
    if (($destination -eq $null) -or ($destination -eq "")) {
        Write-Host("Error: Destination was not provided!")
        Write-Host("You must provide the current application path. This should be the current application root directory")

        Quit
    }

        if (($processId -eq $null) -or ($processId -eq "")) {
        Write-Host("Error: Process ID was not provided!")
        Write-Host("You must provide the running proceess ID. You cannot update the app while stopped.")

        Quit
    }
    
    Write-Host("Update Source Path: " + $source);
    Write-Host("Destination Path: " + $destination);
    Write-Host("Process ID: " + $processId);
    Write-Host("Running as " + $env:UserDomain + "\" + $env:UserName);
    
    if (!(Test-Path -Path $destination -PathType Container)) {
        Write-Host("Error: Destination directory doesn't exist. Quitting.")
        Quit
        }


    $process = Get-Process -Id $processId
    if($process -eq $null){
        Write-Host("Error: A process with process id of "+$processId+" was not found")
        Quit
    }
    $commandLine = (gcim win32_process | Where-Object -Property "processid" -EQ -Value $processId | Select-Object commandline).commandline

    $processFilePath = ($commandLine -split " ")[0]
    if($processFilePath.Contains("w3wp.exe")){
        $hParam = ($commandLine | Select-String -Pattern '-h ".*?"' -AllMatches).Matches.Value -replace '-h ','' -replace '"',''
        $processArguments ='-h "'+$hParam +'"'
    }
    Write-Host "Re-Launch Command "$processFilePath" "$processArguments

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
    Write-Host("Restarting Application")
    Write-Host("Process path: " + $processFilePath)

    if($processArguments -ne $null -and $processArguments -ne "")
    {
        
        Write-Host("Starting with arguments: " + $processArguments)
        $restartedProcess = Start-Process -FilePath $processFilePath -ArgumentList $processArguments -WorkingDirectory $destination -PassThru

    }else{
        $restartedProcess = Start-Process -FilePath $processFilePath -WorkingDirectory $destination -PassThru

    }
    Write-Host("Waiting 15 seconds for Application to restart")

    Start-Sleep -Seconds 15
    Write-Host("Process Stats: "+($restartedProcess| Select-Object *))
    if ($restartedProcess.ExitTime -ne $null) {
        Write-Host("Error: Web Application failed to restart rolling back changes")
        #Perform Rollback Section
        $restoreSource = $backupDirectory + "*"
        Write-Host("Rolling back update")
        Write-Host("Source: " + $restoreSource)
        Write-Host("Destination: " + $destination)
        Copy-Item -Path $restoreSource -Destination $backupDirectory -Recurse -Verbose -Force
        Write-Host("Restarting ApplicationPool")

        if($processArguments -ne $null -and $processArguments -ne "")
        {
            
            Write-Host("Starting with arguments: " + $processArguments)
            $restartedProcess = Start-Process -FilePath $processFilePath -ArgumentList $processArguments -PassThru
    
        }else{
            $restartedProcess = Start-Process -FilePath $processFilePath -PassThru
    
        }
        #$runningSite | Start-IISSite
        Write-Host("Waiting 15 seconds for Application to restart")
    
        Start-Sleep -Seconds 15
        if ($restartedProcess.ExitTime -ne $null) {
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

if($Username -ne "" -and $Password -ne "")
{
$securePassword = ConvertTo-SecureString $Password -AsPlainText -Force
$credential = New-Object System.Management.Automation.PSCredential($Username, $securePassword)
$out = Invoke-Command -ComputerName "localhost" -Credential $credential -ArgumentList  $UpdateSourcePath, $ApplicationDirectory,$ProcessId  -ScriptBlock $updateScript

}
else{
$out = Invoke-Command -ArgumentList  $UpdateSourcePath, $ApplicationDirectory,$ProcessId  -ScriptBlock $updateScript

}
Write-Host($out)
exit


