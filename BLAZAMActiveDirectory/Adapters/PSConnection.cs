using BLAZAM.ActiveDirectory.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.ActiveDirectory.Adapters
{
    internal class PSConnection : IRemoteManagementConnection
    {
        private IADComputer target;
        private IActiveDirectoryContext directory;
        public PSConnection(IActiveDirectoryContext directory, IADComputer target)
        {
            this.target = target;
            this.directory = directory;
        }

        public List<IADComputerDrive> Drives => new();

        public List<ComputerService> Services
        {
            get
            {
                string script = $"Get-CimInstance -ComputerName {target.CanonicalName} -ClassName Win32_Service | Select-Object *";

                List<ComputerService> services = new();
                var results = RunScript(script);

                if (results != null && results.Count > 0)
                {
                    foreach (var service in results)
                    {
                        PSObject pso = service; // We expect one result for the '_Total' instance
                        ComputerService computerService = new();
                        if (pso.Properties["Status"]?.Value != null)
                        {
                            computerService.Status = Convert.ToString(pso.Properties["Status"].Value);
                        }
                        if (pso.Properties["State"]?.Value != null)
                        {
                            computerService.State = Convert.ToString(pso.Properties["State"].Value);
                        }
                        if (pso.Properties["Caption"]?.Value != null)
                        {
                            computerService.Caption = Convert.ToString(pso.Properties["Caption"].Value);
                        }
                        if (pso.Properties["DisplayName"]?.Value != null)
                        {
                            computerService.DisplayName = Convert.ToString(pso.Properties["DisplayName"].Value);
                        }
                        if (pso.Properties["Description"]?.Value != null)
                        {
                            computerService.Description = Convert.ToString(pso.Properties["Description"].Value);
                        }
                        if (pso.Properties["ErrorControl"]?.Value != null)
                        {
                            computerService.ErrorControl = Convert.ToString(pso.Properties["ErrorControl"].Value);
                        }
                        if (pso.Properties["ServiceType"]?.Value != null)
                        {
                            computerService.ServiceType = Convert.ToString(pso.Properties["ServiceType"].Value);
                        }
                        if (pso.Properties["PathName"]?.Value != null)
                        {
                            computerService.PathName = Convert.ToString(pso.Properties["PathName"].Value);
                        }
                        if (pso.Properties["Name"]?.Value != null)
                        {
                            computerService.Name = Convert.ToString(pso.Properties["Name"].Value);
                        }
                        if (pso.Properties["StartMode"]?.Value != null)
                        {
                            computerService.StartMode = Convert.ToString(pso.Properties["StartMode"].Value);
                        }
                        if (pso.Properties["StartName"]?.Value != null)
                        {
                            computerService.StartName = Convert.ToString(pso.Properties["StartName"].Value);
                        }
                        if (pso.Properties["InstallDate"]?.Value != null)
                        {
                            computerService.InstallDate = Convert.ToDateTime(pso.Properties["InstallDate"].Value);
                        }
                        if (pso.Properties["AcceptPause"]?.Value != null)
                        {
                            computerService.CanPause = Convert.ToBoolean(pso.Properties["AcceptPause"].Value);
                        }
                        if (pso.Properties["AcceptStop"]?.Value != null)
                        {
                            computerService.CanStop = Convert.ToBoolean(pso.Properties["AcceptStop"].Value);
                        }
                        if (pso.Properties["Started"]?.Value != null)
                        {
                            computerService.Started = Convert.ToBoolean(pso.Properties["Started"].Value);
                        }
                        services.Add(computerService);
                    }
                }
               
                return services;
            }
        }

        public int Processor
        {
            get
            {
                // The PowerShell command to get overall CPU utilization.
                // It queries the '_Total' instance of the processor performance counters.
                // Note the double curly braces for the Where-Object script block within an interpolated string.
                string script = $"Get-CimInstance -ComputerName {target.CanonicalName} -ClassName Win32_PerfFormattedData_PerfOS_Processor | Where-Object {{$_.Name -eq '_Total'}} | Select-Object PercentProcessorTime";

                var results = RunScript(script);

                if (results != null && results.Count > 0)
                {
                    PSObject pso = results[0]; // We expect one result for the '_Total' instance

                    if (pso.Properties["PercentProcessorTime"]?.Value != null)
                    {
                        // PercentProcessorTime from Win32_PerfFormattedData_PerfOS_Processor is typically a UInt64
                        return Convert.ToInt32(pso.Properties["PercentProcessorTime"].Value);
                    }
                }
                return 0; // Return default if no results, the property is not found, or an error occurred in RunScript
            }
        }
        public ComputerMemory Memory
        {
            get
            {
                // The PowerShell command to get memory information
                // Equivalent to "SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem"
                string script = $"Get-CimInstance -ComputerName {target.CanonicalName} -ClassName Win32_OperatingSystem | Select-Object TotalVisibleMemorySize, FreePhysicalMemory";

                Collection<PSObject> results = RunScript(script);

                if (results != null && results.Count > 0)
                {
                    PSObject pso = results[0]; // We expect only one result from Win32_OperatingSystem

                    // CIM properties are typically uint64 for these values, convert carefully.
                    // TotalVisibleMemorySize is in Kilobytes.
                    // FreePhysicalMemory is in Kilobytes.

                    double total = 0;
                    double free = 0;

                    if (pso.Properties["TotalVisibleMemorySize"]?.Value != null)
                    {
                        total = Convert.ToDouble(pso.Properties["TotalVisibleMemorySize"].Value);
                    }

                    if (pso.Properties["FreePhysicalMemory"]?.Value != null)
                    {
                        free = Convert.ToDouble(pso.Properties["FreePhysicalMemory"].Value);
                    }

                    return new ComputerMemory { Total = total, Free = free };
                }

               

                return new ComputerMemory(); // Return default if no results or an error occurred
            }
        }

        private Collection<PSObject>? RunScript(string script)
        {
            return directory.Impersonation.Run(() => {
                using (PowerShell psInstance = PowerShell.Create())
                {
                    psInstance.AddScript(script);

                    try
                    {
                        Collection<PSObject> psResults = psInstance.Invoke();

                        if (psInstance.HadErrors)
                        {
                            // Log errors from the PowerShell stream
                            foreach (var errorRecord in psInstance.Streams.Error)
                            {
                                // Replace with your actual logging mechanism
                                // Loggers.ActiveDirectoryLogger.Error($"PowerShell script error: {errorRecord.ToString()}");
                                Console.WriteLine($"PowerShell script error: {errorRecord.ToString()}");
                            }
                            return null; // Return default or handle error as appropriate
                        }

                        if (psResults != null && psResults.Count > 0)
                        {
                            return psResults;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log general exceptions during PowerShell execution
                        // Loggers.ActiveDirectoryLogger.Error("PowerShell execution failure: " + ex.Message);
                        Console.WriteLine($"PowerShell execution failure: {ex.Message}");
                    }
                    return null;
                }
            });
            
        }

        

        public List<SharedPrinter> SharePrinters => new();

        public Task<bool> RenameComputerAsync(string newName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ShutdownAsync(int delaySeconds = 0, string? message = null, bool force = true, bool reboot = false)
        {
            throw new NotImplementedException();
        }
    }
}
