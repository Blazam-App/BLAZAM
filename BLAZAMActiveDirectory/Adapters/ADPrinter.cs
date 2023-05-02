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
        //TODO Add the following attributes
        // driverName str
        // driverVersion str
        // location str
        // portName str
        // printBinNames strlist
        // printCollate bool
        // printColor bool
        // printDuplexSupported bool
        // printerName str
        // printOrientationsSupported str
        // printKeepPrintedJobs bool
        // printLanguage str
        // printMediaReady strlist
        // printMediaSupported strlist
        // printMaxResolutionSupported int
        // printMaxXExtent int
        // printMinXExtent int
        // printMinYExtent int
        // printMaxYExtent int
        // printPagesPerMinute int
        // printRate  int
        // printRateUnit str
        // printShareName str
        // printSpooling str
        // printStaplingSupported bool?
        // priority int
        // uNCName string
    }

}