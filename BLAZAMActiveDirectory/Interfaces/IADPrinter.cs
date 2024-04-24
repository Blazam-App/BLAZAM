
using BLAZAM.Database.Models.Permissions;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    public interface IADPrinter : IDirectoryEntryAdapter

    {
        string DriverName { get; set; }
        string DriverVersion { get; set; }
        string Location { get; set; }
        string PortName { get; set; }
        string PrinterName { get; set; }
        string ShortServerName { get; set; }
        string ServerName { get; set; }
        string PrintLanguage { get; set; }
        string PrintRateUnit { get; set; }
        string UncName { get; set; }
        int VersionNumber { get; set; }
    }
}