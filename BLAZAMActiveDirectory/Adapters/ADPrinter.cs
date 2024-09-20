using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Models.Permissions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Contracts;
using System.DirectoryServices;
using System.Reflection.PortableExecutable;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class ADPrinter : DirectoryEntryAdapter, IADPrinter
    {
        public string DriverName
        {

            get
            {
                return GetStringProperty("driverName");
            }
            set
            {
                SetProperty("driverName", value);
            }
        }
        public string DriverVersion
        {

            get
            {
                return GetStringProperty("driverVersion");
            }
            set
            {
                SetProperty("driverVersion", value);
            }
        }
        public string Location
        {

            get
            {
                return GetStringProperty("location");
            }
            set
            {
                SetProperty("location", value);
            }
        }
        public string PortName
        {

            get
            {
                return GetStringProperty("portName");
            }
            set
            {
                SetProperty("portName", value);
            }
        }
        public string PrinterName
        {

            get
            {
                return GetStringProperty("printerName");
            }
            set
            {
                SetProperty("printerName", value);
            }
        }

        public string ShortServerName
        {

            get
            {
                return GetStringProperty("shortServerName");
            }
            set
            {
                SetProperty("shortServerName", value);
            }
        }
        public string ServerName
        {

            get
            {
                return GetStringProperty("serverName");
            }
            set
            {
                SetProperty("serverName", value);
            }
        }

        public string PrintLanguage
        {

            get
            {
                return GetStringProperty("printLanguage");
            }
            set
            {
                SetProperty("printLanguage", value);
            }
        }

        public string PrintRateUnit
        {

            get
            {
                return GetStringProperty("printRateUnit");
            }
            set
            {
                SetProperty("printRateUnit", value);
            }
        }

        public string PrintShareName
        {

            get
            {
                return GetStringProperty("printShareName");
            }
            set
            {
                SetProperty("printShareName", value);
            }
        }

        public string UncName
        {

            get
            {
                return GetStringProperty("uNCName");
            }
            set
            {
                SetProperty("uNCName", value);
            }
        }

        public List<string> PrintBinNames
        {
            get
            {
                return GetStringListProperty("printBinNames");
            }
            set
            {
                SetProperty("printBinNames", value);
            }
        }

        public bool PrintCollate
        {
            get
            {
                return GetProperty<bool>("printCollate");
            }
            set
            {
                SetProperty("printCollate", value);
            }
        }

        public bool PrintColor
        {
            get
            {
                return GetProperty<bool>("printColor");
            }
            set
            {
                SetProperty("printColor", value);
            }
        }

        public bool PrintDuplexSupported
        {
            get
            {
                return GetProperty<bool>("printDuplexSupported");
            }
            set
            {
                SetProperty("printDuplexSupported", value);
            }
        }

        public string PrintOrientationsSupported
        {
            get
            {
                return GetStringProperty("printOrientationsSupported");
            }
            set
            {
                SetProperty("printOrientationsSupported", value);
            }
        }

        public bool PrintKeepPrintedJobs
        {
            get
            {
                return GetProperty<bool>("printKeepPrintedJobs");
            }
            set
            {
                SetProperty("printKeepPrintedJobs", value);
            }
        }

        public List<string> PrintMediaReady
        {
            get
            {
                return GetStringListProperty("printMediaReady");
            }
            set
            {
                SetProperty("printMediaReady", value);
            }
        }

        public List<string> PrintMediaSupported
        {
            get
            {
                return GetStringListProperty("printMediaSupported");
            }
            set
            {
                SetProperty("printMediaSupported", value);
            }
        }

        public int PrintMaxResolutionSupported
        {
            get
            {
                return GetProperty<int>("printMaxResolutionSupported");
            }
            set
            {
                SetProperty("printMaxResolutionSupported", value);
            }
        }

        public int PrintMaxXExtent
        {
            get
            {
                return GetProperty<int>("printMaxXExtent");
            }
            set
            {
                SetProperty("printMaxXExtent", value);
            }
        }

        public int PrintMinXExtent
        {
            get
            {
                return GetProperty<int>("printMinXExtent");
            }
            set
            {
                SetProperty("printMinXExtent", value);
            }
        }

        public int PrintMinYExtent
        {
            get
            {
                return GetProperty<int>("printMinYExtent");
            }
            set
            {
                SetProperty("printMinYExtent", value);
            }
        }

        public int PrintMaxYExtent
        {
            get
            {
                return GetProperty<int>("printMaxYExtent");
            }
            set
            {
                SetProperty("printMaxYExtent", value);
            }
        }

        public int PrintPagesPerMinute
        {
            get
            {
                return GetProperty<int>("printPagesPerMinute");
            }
            set
            {
                SetProperty("printPagesPerMinute", value);
            }
        }

        public int PrintRate
        {
            get
            {
                return GetProperty<int>("printRate");
            }
            set
            {
                SetProperty("printRate", value);
            }
        }

        public string PrintSpooling
        {
            get
            {
                return GetStringProperty("printSpooling");
            }
            set
            {
                SetProperty("printSpooling", value);
            }
        }

        public bool PrintStaplingSupported
        {
            get
            {
                return GetProperty<bool>("printStaplingSupported");
            }
            set
            {
                SetProperty("printStaplingSupported", value);
            }
        }

        public int Priority
        {
            get
            {
                return GetProperty<int>("priority");
            }
            set
            {
                SetProperty("priority", value);
            }
        }
        public int VersionNumber
        {
            get
            {
                return GetProperty<int>("versionNumber");
            }
            set
            {
                SetProperty("versionNumber", value);
            }
        }

    }

}