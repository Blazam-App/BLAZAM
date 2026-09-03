namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// Represents an Active Directory printer object with its associated properties.
    /// </summary>
    public interface IADPrinter : IDirectoryEntryAdapter
    {
        /// <summary>
        /// Gets or sets the name of the printer driver.
        /// </summary>
        string? DriverName { get; set; }

        /// <summary>
        /// Gets or sets the version of the printer driver.
        /// </summary>
        string? DriverVersion { get; set; }

        /// <summary>
        /// Gets or sets the physical location of the printer.
        /// </summary>
        string? Location { get; set; }

        /// <summary>
        /// Gets or sets the port name the printer is connected to.
        /// </summary>
        string? PortName { get; set; }

        /// <summary>
        /// Gets or sets the name of the printer.
        /// </summary>
        string? PrinterName { get; set; }

        /// <summary>
        /// Gets or sets the short name of the server hosting the printer.
        /// </summary>
        string? ShortServerName { get; set; }

        /// <summary>
        /// Gets or sets the full name of the server hosting the printer.
        /// </summary>
        string? ServerName { get; set; }

        /// <summary>
        /// Gets or sets the printer language (e.g., PostScript, PCL).
        /// </summary>
        string? PrintLanguage { get; set; }

        /// <summary>
        /// Gets or sets the unit of measurement for the print rate (e.g., pages per minute).
        /// </summary>
        string? PrintRateUnit { get; set; }

        /// <summary>
        /// Gets or sets the Universal Naming Convention (UNC) path to the printer.
        /// </summary>
        string? UncName { get; set; }

        /// <summary>
        /// Gets or sets the version number of the printer.
        /// </summary>
        int VersionNumber { get; set; }
    }
}