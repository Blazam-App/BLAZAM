using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class WSManager
    {
        private void InitializeRunspace(bool skipCertificateCheck)
        {
            var connectionUri = new Uri($"{(_useHttps ? "https://" : "http://")}{_computerName}:{_port}/wsman");
            var connectionInfo = new WSManConnectionInfo(connectionUri, "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", _credential);

            // Important: For production, avoid skipping certificate checks with HTTPS.
            // This is primarily for development/testing with self-signed certificates.
            if (_useHttps && skipCertificateCheck)
            {
                connectionInfo.SkipCACheck = true;
                connectionInfo.SkipCNCheck = true;
                // For more granular control, you might need to handle ServerCertificateValidationCallback
                // System.Net.ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);
                // Be very careful with the above line as it bypasses all SSL certificate validation globally.
            }

            // Set operation timeouts if needed
            // connectionInfo.OperationTimeout = 4 * 60 * 1000; // 4 minutes
            // connectionInfo.OpenTimeout = 1 * 60 * 1000; // 1 minute

            _runspace = RunspaceFactory.CreateRunspace(connectionInfo);
            try
            {
                _runspace.Open();
            }
            catch (Exception ex)
            {
                // Loggers.YourLogger.Error(ex, $"Failed to open runspace to {_computerName}");
                throw new InvalidOperationException($"Failed to open PowerShell runspace to '{_computerName}'. Ensure WinRM is configured and accessible. Error: {ex.Message}", ex);
            }
        }

        private async Task<Collection<PSObject>> ExecuteScriptAsync(string script, Dictionary<string, object>? parameters = null)
        {
            if (_runspace == null || _runspace.RunspaceStateInfo.State != RunspaceState.Opened)
            {
                throw new InvalidOperationException("Runspace is not open or in a valid state.");
            }

            using (PowerShell ps = PowerShell.Create())
            {
                ps.Runspace = _runspace;
                ps.AddScript(script);
                if (parameters != null)
                {
                    ps.AddParameters(parameters);
                }

                // Log the script being executed (optional, for debugging)
                // Loggers.YourLogger.Debug($"Executing PowerShell on {_computerName}: {script}");

                Collection<PSObject> results;
                try
                {
                    results = await Task.Factory.FromAsync(ps.BeginInvoke(), ps.EndInvoke).ConfigureAwait(false);

                    if (ps.Streams.Error.Count > 0)
                    {
                        var errors = new StringBuilder();
                        foreach (var error in ps.Streams.Error)
                        {
                            errors.AppendLine(error.ToString());
                        }
                        // Loggers.YourLogger.Error($"PowerShell errors on {_computerName} for script '{script}': {errors.ToString()}");
                        throw new Exception($"PowerShell script execution failed with errors: {errors.ToString()}");
                    }
                }
                catch (Exception ex)
                {
                    // Loggers.YourLogger.Error(ex, $"Error executing PowerShell script on {_computerName}: {script}");
                    throw; // Re-throw to allow caller to handle
                }
                return results ?? new Collection<PSObject>();
            }
        }

        private T? GetPropertyValue<T>(PSObject pso, string propertyName)
        {
            var property = pso.Properties[propertyName];
            if (property == null || property.Value == null)
            {
                return default;
            }

            try
            {
                object val = property.Value;
                if (val is PSObject psVal) // Unwrap if it's a PSObject itself
                {
                    val = psVal.BaseObject;
                }

                if (val is PSCustomObject && typeof(T) != typeof(PSCustomObject))
                {
                    // If T is not PSCustomObject, we might not be able to convert directly.
                    // This case might need specific handling based on the expected type T.
                    // For now, return default if direct conversion fails.
                    return default;
                }


                var targetType = typeof(T);
                var underlyingType = Nullable.GetUnderlyingType(targetType);
                if (underlyingType != null) // It's a Nullable<T>
                {
                    if (val == null) return default; // Already handled by property.Value == null check, but good to be explicit
                    return (T)Convert.ChangeType(val, underlyingType);
                }
                return (T)Convert.ChangeType(val, targetType);
            }
            catch (InvalidCastException ex)
            {
                // Loggers.YourLogger.Warning($"Invalid cast for property '{propertyName}' to type '{typeof(T)}' on computer '{_computerName}'. Value: '{property.Value}'. Error: {ex.Message}");
                return default;
            }
            catch (FormatException ex)
            {
                // Loggers.YourLogger.Warning($"Format exception for property '{propertyName}' to type '{typeof(T)}' on computer '{_computerName}'. Value: '{property.Value}'. Error: {ex.Message}");
                return default;
            }
        }

        // --- Methods mirroring WmiConnection functionality ---

        public async Task<List<ADComputerDrive>> GetLogicalDisksAsync()
        {
            // Win32_LogicalDisk properties: DeviceID, FreeSpace, Size, Description, DriveType, FileSystem, MediaType, VolumeDirty, VolumeSerialNumber
            string script = "Get-CimInstance -ClassName Win32_LogicalDisk | Select-Object DeviceID, FreeSpace, Size, Description, DriveType, FileSystem, MediaType, VolumeDirty, VolumeSerialNumber";
            var psObjects = await ExecuteScriptAsync(script);
            var drives = new List<ADComputerDrive>();

            foreach (var pso in psObjects)
            {
                drives.Add(new ADComputerDrive
                {
                    Letter = GetPropertyValue<string>(pso, "DeviceID"),
                    FreeSpace = GetPropertyValue<double>(pso, "FreeSpace") / (1024 * 1024 * 1024),
                    Capacity = GetPropertyValue<double>(pso, "Size") / (1024 * 1024 * 1024),
                    Description = GetPropertyValue<string>(pso, "Description"),
                    DriveType = (DriveType)GetPropertyValue<int>(pso, "DriveType"),
                    FileSystem = GetPropertyValue<string>(pso, "FileSystem"),
                    MediaType = GetPropertyValue<int>(pso, "MediaType"),
                    Dirty = GetPropertyValue<bool>(pso, "VolumeDirty"),
                    Serial = GetPropertyValue<string>(pso, "VolumeSerialNumber")
                });
            }
            return drives;
        }

        public async Task<ComputerMemory?> GetTotalMemoryAsync()
        {
            // Win32_OperatingSystem properties: TotalVisibleMemorySize, FreePhysicalMemory (both in KB)
            string script = "Get-CimInstance -ClassName Win32_OperatingSystem | Select-Object TotalVisibleMemorySize, FreePhysicalMemory";
            var psObjects = await ExecuteScriptAsync(script);

            if (psObjects.Count > 0)
            {
                var pso = psObjects[0];
                
                return new ComputerMemory
                {
                    Total = GetPropertyValue<double>(pso, "TotalVisibleMemorySize"),
                    Free = GetPropertyValue<double>(pso, "FreePhysicalMemory")
                };
            }
            return null;
        }

        public async Task<int?> GetCpuStatsAsync()
        {
            // Win32_PerfFormattedData_PerfOS_Processor WHERE Name='_Total'
            // Properties: PercentProcessorTime, Name
            string script = "Get-CimInstance -ClassName Win32_PerfFormattedData_PerfOS_Processor -Filter \"Name='_Total'\" | Select-Object Name, PercentProcessorTime";
            var psObjects = await ExecuteScriptAsync(script);

            if (psObjects.Count > 0)
            {
                var pso = psObjects[0];
                int percentProcessor = Convert.ToInt32(GetPropertyValue<double>(pso, "PercentProcessorTime"));
                return percentProcessor;
               
            }
            return null;
        }

       

        public async Task<List<ComputerService>> GetServicesAsync()
        {
            // Win32_Service properties: Name, DisplayName, State, Status, StartMode, ProcessId, PathName, Description, ServiceType, AcceptStop, AcceptPause
            string script = "Get-CimInstance -ClassName Win32_Service | Select-Object Name, DisplayName, State, Status, StartMode, ProcessId, PathName, Description, ServiceType, AcceptStop, AcceptPause";
            var psObjects = await ExecuteScriptAsync(script);
            var services = new List<ComputerService>();

            foreach (var pso in psObjects)
            {
                services.Add(new ComputerService
                {
                    Name = GetPropertyValue<string>(pso, "Name"),
                    DisplayName = GetPropertyValue<string>(pso, "DisplayName"),
                    State = GetPropertyValue<string>(pso, "State"),
                    Status = GetPropertyValue<string>(pso, "Status"),
                    StartMode = GetPropertyValue<string>(pso, "StartMode"),
                    ProcessId = GetPropertyValue<uint?>(pso, "ProcessId"),
                    PathName = GetPropertyValue<string>(pso, "PathName"),
                    Description = GetPropertyValue<string>(pso, "Description"),
                    ServiceType = GetPropertyValue<string>(pso, "ServiceType"),
                });
            }
            return services;
        }

       

        public async Task<bool> RenameComputerAsync(string newName, string? domainAdminUser = null, string? domainAdminPassword = null, bool restart = false)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentException("New computer name cannot be empty.", nameof(newName));
            }

            // The Win32_ComputerSystem Rename method is instance-based.
            // We need to get the instance first.
            // Arguments for Rename: NewName (string), Password (string), UserName (string)
            // Password and UserName are for joining a domain during the rename.

            string script = @"
                param(
                    [string]$NewName,
                    [string]$DomainAdminUser,
                    [string]$DomainAdminPassword,
                    [bool]$Restart
                )
                try {
                    $compSystem = Get-CimInstance -ClassName Win32_ComputerSystem
                    $params = @{ Name = $NewName }
                    if ($DomainAdminUser -and $DomainAdminPassword) {
                        $params.UserName = $DomainAdminUser
                        $params.Password = $DomainAdminPassword 
                        # For domain join, you might need FQDN for username: e.g., 'DOMAIN\User'
                    }
                    Invoke-CimMethod -InputObject $compSystem -MethodName Rename -Arguments $params
                    if ($Restart) {
                        Restart-Computer -Force
                    }
                    return $true
                } catch {
                    # Write-Error $_.Exception.Message # This goes to PowerShell error stream
                    # Consider how to propagate this error message back
                    return $false
                }
            ";

            var parameters = new Dictionary<string, object>
            {
                { "NewName", newName },
                { "Restart", restart }
            };
            if (!string.IsNullOrEmpty(domainAdminUser) && !string.IsNullOrEmpty(domainAdminPassword))
            {
                parameters.Add("DomainAdminUser", domainAdminUser);
                parameters.Add("DomainAdminPassword", domainAdminPassword);
            }

            try
            {
                var results = await ExecuteScriptAsync(script, parameters);
                // If the script returns $true or $false explicitly
                if (results.Count > 0 && results[0].BaseObject is bool success)
                {
                    return success;
                }
                // If Invoke-CimMethod succeeded without error, it implies success (though it returns a return code object)
                // For simplicity, if no exception is thrown by ExecuteScriptAsync, we assume success here.
                // A more robust solution would inspect the return value of Invoke-CimMethod if needed.
                return true;
            }
            catch (Exception ex)
            {
                // Loggers.YourLogger.Error(ex, $"Failed to rename computer {_computerName} to {newName}");
                return false;
            }
        }

        /// <summary>
        /// Invokes a CIM method on a specified CIM class or instance.
        /// </summary>
        /// <param name="className">The CIM class name.</param>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="methodParameters">A dictionary of parameters for the method.</param>
        /// <param name="instanceFilter">Optional WQL filter to target a specific instance.</param>
        /// <returns>The result of the method invocation, typically a PSObject containing ReturnValue and other output.</returns>
        public async Task<PSObject?> InvokeCimMethodAsync(string className, string methodName, Dictionary<string, object> methodParameters, string? instanceFilter = null)
        {
            var scriptBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(instanceFilter))
            {
                scriptBuilder.AppendLine($"$instance = Get-CimInstance -ClassName {className} -Filter \"{instanceFilter.Replace("\"", "`\"")}\""); // Escape quotes in filter
                scriptBuilder.AppendLine($"Invoke-CimMethod -InputObject $instance -MethodName {methodName} -Arguments $params");
            }
            else // Class static method
            {
                scriptBuilder.AppendLine($"Invoke-CimMethod -ClassName {className} -MethodName {methodName} -Arguments $params");
            }

            var parameters = new Dictionary<string, object> { { "params", methodParameters } };

            var results = await ExecuteScriptAsync(scriptBuilder.ToString(), parameters);
            return results.FirstOrDefault();
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_runspace != null)
                {
                    if (_runspace.RunspaceStateInfo.State == RunspaceState.Opened)
                    {
                        _runspace.Close();
                    }
                    _runspace.Dispose();
                    _runspace = null;
                }
            }
        }

        ~WSManager()
        {
            Dispose(false);
        }
    }
}
