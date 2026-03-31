#Name BLAZAM Update Task Registration Script
#Purpose Registers a Windows Scheduled Task to run the BLAZAM updater at 2AM daily
#Version 1.0.0

$taskName    = "BLAZAM Auto Updater"
$taskDesc    = "Runs the BLAZAM update.ps1 script daily at 2:00 AM."
$scriptPath  = Join-Path $PSScriptRoot "update.ps1"
$triggerTime = "02:00"

Write-Host ""
Write-Host "========================================="
Write-Host "  BLAZAM Scheduled Update Task Setup"
Write-Host "========================================="
Write-Host ""
Write-Host "This will register a scheduled task to run:"
Write-Host "  $scriptPath"
Write-Host "daily at $triggerTime local time."
Write-Host ""

if (!(Test-Path -Path $scriptPath -PathType Leaf)) {
    Write-Error "update.ps1 not found at expected path: $scriptPath"
    exit 1
}

# Prompt for alternate credentials
$useCustomCreds = $null
while ($useCustomCreds -notin @("y", "n")) {
    $useCustomCreds = (Read-Host "Run the task as a specific user instead of SYSTEM? (y/n)").Trim().ToLower()
}

if ($useCustomCreds -eq "y") {
    $taskUser     = Read-Host "Enter the username (e.g. DOMAIN\ServiceAccount or .\LocalUser)"
    $taskPassword = Read-Host "Enter the password for '$taskUser'" -AsSecureString
    $plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($taskPassword)
    )
    $runLevel = "Highest"
    $logonType = "Password"
} else {
    $taskUser      = "SYSTEM"
    $plainPassword = $null
    $runLevel      = "Highest"
    $logonType     = "ServiceAccount"
}

Write-Host ""
Write-Host "Registering scheduled task '$taskName'..."

# Build the task components
$action  = New-ScheduledTaskAction `
    -Execute "powershell.exe" `
    -Argument "-NonInteractive -NoProfile -ExecutionPolicy Bypass -File `"$scriptPath`""

$trigger = New-ScheduledTaskTrigger -Daily -At $triggerTime

$settings = New-ScheduledTaskSettingsSet `
    -ExecutionTimeLimit (New-TimeSpan -Hours 1) `
    -StartWhenAvailable `
    -MultipleInstances IgnoreNew

$principal = New-ScheduledTaskPrincipal `
    -UserId    $taskUser `
    -RunLevel  $runLevel `
    -LogonType $logonType

# Remove existing task if present
if (Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue) {
    Write-Host "Existing task '$taskName' found. Removing before re-registering..."
    Unregister-ScheduledTask -TaskName $taskName -Confirm:$false
}

# Register the task
try {
    if ($useCustomCreds -eq "y") {
        Register-ScheduledTask `
            -TaskName   $taskName `
            -Description $taskDesc `
            -Action     $action `
            -Trigger    $trigger `
            -Settings   $settings `
            -Principal  $principal `
            -Password   $plainPassword `
            -Force | Out-Null
    } else {
        Register-ScheduledTask `
            -TaskName    $taskName `
            -Description $taskDesc `
            -Action      $action `
            -Trigger     $trigger `
            -Settings    $settings `
            -Principal   $principal `
            -Force | Out-Null
    }

    Write-Host ""
    Write-Host "Scheduled task '$taskName' registered successfully." -ForegroundColor Green
    Write-Host "It will run daily at $triggerTime as: $taskUser"
} catch {
    Write-Error "Failed to register scheduled task: $_"
    exit 1
} finally {
    # Clear the plaintext password from memory
    if ($null -ne $plainPassword) {
        $plainPassword = $null
        [System.GC]::Collect()
    }
}

Write-Host ""