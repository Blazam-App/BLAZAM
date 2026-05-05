using BLAZAM.ActiveDirectory.Helpers;
using BLAZAM.ActiveDirectory.Interfaces;
using System.Management;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class SharedPrinter
    {
        public IADPrinter? ADPrinter { get; set; }

        private readonly IADComputer _host;
        public IADComputer Host => _host;
        private readonly ManagementObject _wmiPrinterObject;

        public SharedPrinter(IADComputer host, ManagementObject wmiPrinterObject)
        {
            _host = host;
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
            var directory = _host.Directory;
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

        public bool DoCompleteFirst => _wmiPrinterObject.GetPropertyValue<bool>(nameof(DoCompleteFirst));
        public bool Hidden => _wmiPrinterObject.GetPropertyValue<bool>(nameof(Hidden));
        public bool Local => _wmiPrinterObject.GetPropertyValue<bool>(nameof(Local));
        public bool Network => _wmiPrinterObject.GetPropertyValue<bool>(nameof(Network));
        public bool SpoolEnabled => _wmiPrinterObject.GetPropertyValue<bool>(nameof(SpoolEnabled));
        public bool Published => _wmiPrinterObject.GetPropertyValue<bool>(nameof(Published));
        public bool Queued => _wmiPrinterObject.GetPropertyValue<bool>(nameof(Queued));
        public string DriverName => _wmiPrinterObject.GetPropertyValue<string>(nameof(DriverName));
        public string ShareName => _wmiPrinterObject.GetPropertyValue<string>(nameof(ShareName));
        public string ErrorDescription => _wmiPrinterObject.GetPropertyValue<string>(nameof(ErrorDescription));
        public string ErrorInformation => _wmiPrinterObject.GetPropertyValue<string>(nameof(ErrorInformation));
        public string PortName => _wmiPrinterObject.GetPropertyValue<string>(nameof(PortName));
        public string Location => _wmiPrinterObject.GetPropertyValue<string>(nameof(Location));
        public string Comment => _wmiPrinterObject.GetPropertyValue<string>(nameof(Comment));
        public string Caption => _wmiPrinterObject.GetPropertyValue<string>(nameof(Caption));
        public string Description => _wmiPrinterObject.GetPropertyValue<string>(nameof(Description));
        public string Name => _wmiPrinterObject.GetPropertyValue<string>(nameof(Name));
        public UInt32 PrinterState => _wmiPrinterObject.GetPropertyValue<UInt32>(nameof(PrinterState));
        public UInt16 PrinterStatus => _wmiPrinterObject.GetPropertyValue<UInt16>(nameof(PrinterStatus));
    }
}
