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
        //TODO Add the following attributes
        // printBinNames strlist
        // printCollate bool
        // printColor bool
        // printDuplexSupported bool
        // printOrientationsSupported str
        // printKeepPrintedJobs bool
        // printMediaReady strlist
        // printMediaSupported strlist
        // printMaxResolutionSupported int
        // printMaxXExtent int
        // printMinXExtent int
        // printMinYExtent int
        // printMaxYExtent int
        // printPagesPerMinute int
        // printRate  int
        // printSpooling str
        // printStaplingSupported bool?
        // priority int
        // uNCName string
    }

}