using BLAZAM.ActiveDirectory.Interfaces;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class ADPrinter : DirectoryEntryAdapter, IADPrinter
    {
        public string DriverName
        {

            get
            {
                return GetStringAttribute("driverName");
            }
            set
            {
                SetAttribute("driverName", value);
            }
        }
        public string DriverVersion
        {

            get
            {
                return GetStringAttribute("driverVersion");
            }
            set
            {
                SetAttribute("driverVersion", value);
            }
        }
        public string Location
        {

            get
            {
                return GetStringAttribute("location");
            }
            set
            {
                SetAttribute("location", value);
            }
        }
        public string PortName
        {

            get
            {
                return GetStringAttribute("portName");
            }
            set
            {
                SetAttribute("portName", value);
            }
        }
        public string PrinterName
        {

            get
            {
                return GetStringAttribute("printerName");
            }
            set
            {
                SetAttribute("printerName", value);
            }
        }

        public string ShortServerName
        {

            get
            {
                return GetStringAttribute("shortServerName");
            }
            set
            {
                SetAttribute("shortServerName", value);
            }
        }
        public string ServerName
        {

            get
            {
                return GetStringAttribute("serverName");
            }
            set
            {
                SetAttribute("serverName", value);
            }
        }

        public string PrintLanguage
        {

            get
            {
                return GetStringAttribute("printLanguage");
            }
            set
            {
                SetAttribute("printLanguage", value);
            }
        }

        public string PrintRateUnit
        {

            get
            {
                return GetStringAttribute("printRateUnit");
            }
            set
            {
                SetAttribute("printRateUnit", value);
            }
        }

        public string PrintShareName
        {

            get
            {
                return GetStringAttribute("printShareName");
            }
            set
            {
                SetAttribute("printShareName", value);
            }
        }

        public string UncName
        {

            get
            {
                return GetStringAttribute("uNCName");
            }
            set
            {
                SetAttribute("uNCName", value);
            }
        }

        public List<string> PrintBinNames
        {
            get
            {
                return GetStringListAttribute("printBinNames");
            }
            set
            {
                SetAttribute("printBinNames", value);
            }
        }

        public bool PrintCollate
        {
            get
            {
                return GetAttribute<bool>("printCollate");
            }
            set
            {
                SetAttribute("printCollate", value);
            }
        }

        public bool PrintColor
        {
            get
            {
                return GetAttribute<bool>("printColor");
            }
            set
            {
                SetAttribute("printColor", value);
            }
        }

        public bool PrintDuplexSupported
        {
            get
            {
                return GetAttribute<bool>("printDuplexSupported");
            }
            set
            {
                SetAttribute("printDuplexSupported", value);
            }
        }

        public string PrintOrientationsSupported
        {
            get
            {
                return GetStringAttribute("printOrientationsSupported");
            }
            set
            {
                SetAttribute("printOrientationsSupported", value);
            }
        }

        public bool PrintKeepPrintedJobs
        {
            get
            {
                return GetAttribute<bool>("printKeepPrintedJobs");
            }
            set
            {
                SetAttribute("printKeepPrintedJobs", value);
            }
        }

        public List<string> PrintMediaReady
        {
            get
            {
                return GetStringListAttribute("printMediaReady");
            }
            set
            {
                SetAttribute("printMediaReady", value);
            }
        }

        public List<string> PrintMediaSupported
        {
            get
            {
                return GetStringListAttribute("printMediaSupported");
            }
            set
            {
                SetAttribute("printMediaSupported", value);
            }
        }

        public int PrintMaxResolutionSupported
        {
            get
            {
                return GetAttribute<int>("printMaxResolutionSupported");
            }
            set
            {
                SetAttribute("printMaxResolutionSupported", value);
            }
        }

        public int PrintMaxXExtent
        {
            get
            {
                return GetAttribute<int>("printMaxXExtent");
            }
            set
            {
                SetAttribute("printMaxXExtent", value);
            }
        }

        public int PrintMinXExtent
        {
            get
            {
                return GetAttribute<int>("printMinXExtent");
            }
            set
            {
                SetAttribute("printMinXExtent", value);
            }
        }

        public int PrintMinYExtent
        {
            get
            {
                return GetAttribute<int>("printMinYExtent");
            }
            set
            {
                SetAttribute("printMinYExtent", value);
            }
        }

        public int PrintMaxYExtent
        {
            get
            {
                return GetAttribute<int>("printMaxYExtent");
            }
            set
            {
                SetAttribute("printMaxYExtent", value);
            }
        }

        public int PrintPagesPerMinute
        {
            get
            {
                return GetAttribute<int>("printPagesPerMinute");
            }
            set
            {
                SetAttribute("printPagesPerMinute", value);
            }
        }

        public int PrintRate
        {
            get
            {
                return GetAttribute<int>("printRate");
            }
            set
            {
                SetAttribute("printRate", value);
            }
        }

        public string PrintSpooling
        {
            get
            {
                return GetStringAttribute("printSpooling");
            }
            set
            {
                SetAttribute("printSpooling", value);
            }
        }

        public bool PrintStaplingSupported
        {
            get
            {
                return GetAttribute<bool>("printStaplingSupported");
            }
            set
            {
                SetAttribute("printStaplingSupported", value);
            }
        }

        public int Priority
        {
            get
            {
                return GetAttribute<int>("priority");
            }
            set
            {
                SetAttribute("priority", value);
            }
        }
        public int VersionNumber
        {
            get
            {
                return GetAttribute<int>("versionNumber");
            }
            set
            {
                SetAttribute("versionNumber", value);
            }
        }

    }

}