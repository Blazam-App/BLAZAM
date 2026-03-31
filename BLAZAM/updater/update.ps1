#Name BLAZAM Updater Script
#Purpose Provides a decoupled way to elevate, and update web app
#Version 1.2.0

Write-Host("Performs a self update of the BLAZAM Web Application.")
Write-Host("Usage:`r`n updater.ps1")

function Quit {
    Stop-Transcript -ErrorAction SilentlyContinue
    exit
}

function StopApp {
    New-Item -Path $global:destination -Name "app_offline.htm" -ItemType "file" -Force
    try {
        Stop-Service -Name "Blazam" -Force -ErrorAction Stop
        Write-Host("Blazam service stopped successfully.")
    } catch {
        Write-Host("Warning: Failed to stop Blazam service - " + $_.Exception.Message)
        Write-Host("Continuing with update anyway...")
    }
    Start-Sleep -Seconds 15
}

function StartApp {
    Remove-Item -Path $global:destination\app_offline.htm -Force
    Start-Service -Name "Blazam" -ErrorAction Continue
    Write-Host("Waiting 15 seconds for Application to restart")
    Start-Sleep -Seconds 15
}

function PerformBackup {
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

function ApplyUpdate {
    #Apply Update Section
    $updateSource = $global:source + "\*"
    Write-Host("Applying Update")
    Write-Host("Source: " + $updateSource)
    Write-Host("Destination: " + $global:destination)
    Copy-Item -Path $updateSource -Destination $global:destination -Exclude "*\updater\*" -Recurse -Verbose -Force

    Start-Sleep -Seconds 2
}

$global:source = Join-Path $PSScriptRoot "staged"
$global:destination = (Get-Item (Join-Path $PSScriptRoot "..")).FullName + "\"

$logPath = $global:destination + "updater\lastUpdateAttempt.txt"

Write-Host("Update Source Path: " + $global:source)
Write-Host("Destination Path: " + $global:destination)
Write-Host("Log path: " + $logPath)

Start-Transcript -Path $logPath

Write-Host("Running as " + $env:UserDomain + "\" + $env:UserName)

if (!(Test-Path -Path $global:source -PathType Container)) {
    Write-Host("Error: Source directory '" + $global:source + "' does not exist. Quitting.")
    Quit
}

if (!(Test-Path -Path $global:destination -PathType Container)) {
    Write-Host("Error: Destination directory does not exist. Quitting.")
    Quit
}

StopApp

ApplyUpdate

Write-Host("Restarting Application")
StartApp

Write-Host("Web Application successfully updated")
Stop-Transcript
exit


