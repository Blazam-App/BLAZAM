using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class SharedPrinter
    {
        public IADPrinter? ADPrinter { get; set; }

        public IADComputer Host;
        private ManagementObject _wmiPrinterObject;

        public SharedPrinter(IADComputer host, ManagementObject wmiPrinterObject)
        {
            Host = host;
            _wmiPrinterObject = wmiPrinterObject;
            GetDirectoryPrinter();
        }

        /// <summary>
        /// Returns the mathing printer in Active Directory
        /// if one exists, otherwise returns null.
        /// </summary>
        /// <returns></returns>
        public void GetDirectoryPrinter()
        {
            var directory = Host.Directory;
            if (directory != null)
            {
                var printer = directory.Printers.FindPrintersByString(ShareName).FirstOrDefault();
                if (printer != null)
                {

                    ADPrinter = printer;
                }
            }
            return;
        }

        public bool DoCompleteFirst => _wmiPrinterObject.GetPropertyValue<bool>("DoCompleteFirst");
        public bool Hidden => _wmiPrinterObject.GetPropertyValue<bool>("Hidden");
        public bool Local => _wmiPrinterObject.GetPropertyValue<bool>("Local");
        public bool Network => _wmiPrinterObject.GetPropertyValue<bool>("Network");
        public bool SpoolEnabled => _wmiPrinterObject.GetPropertyValue<bool>("SpoolEnabled");
        public bool Published => _wmiPrinterObject.GetPropertyValue<bool>("Published");
        public bool Queued => _wmiPrinterObject.GetPropertyValue<bool>("Queued");
        public string DriverName => _wmiPrinterObject.GetPropertyValue<string>("DriverName");
        public string ShareName => _wmiPrinterObject.GetPropertyValue<string>("ShareName");
        public string ErrorDescription => _wmiPrinterObject.GetPropertyValue<string>("ErrorDescription");
        public string ErrorInformation => _wmiPrinterObject.GetPropertyValue<string>("ErrorInformation");
        public string PortName => _wmiPrinterObject.GetPropertyValue<string>("PortName");
        public string Location => _wmiPrinterObject.GetPropertyValue<string>("Location");
        public string Comment => _wmiPrinterObject.GetPropertyValue<string>("Comment");
        public string Caption => _wmiPrinterObject.GetPropertyValue<string>("Caption");
        public string Description => _wmiPrinterObject.GetPropertyValue<string>("Description");
        public string Name => _wmiPrinterObject.GetPropertyValue<string>("Name");
        public UInt32 PrinterState => _wmiPrinterObject.GetPropertyValue<UInt32>("PrinterState");
        public UInt16 PrinterStatus => _wmiPrinterObject.GetPropertyValue<UInt16>("PrinterStatus");
    }
}
