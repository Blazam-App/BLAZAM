using BLAZAM.ActiveDirectory.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.ActiveDirectory.Data
{
    public class DomainControllerEventLogReader
    {
        private readonly IActiveDirectoryContext _directory;

        public DomainControllerEventLogReader(IActiveDirectoryContext directory)
        {
            _directory = directory;
        }

        public List<EventLogEntry> GetWorkstationLogonEvents(DateTime startTime, DateTime endTime)
        {
            var events = new List<EventLogEntry>();

            foreach (var domainController in _directory.DomainControllers)
            {
                try
                {
                    var entry = domainController.GetDirectoryEntry();
                    var searcher = new DirectorySearcher(entry)
                    {
                        Filter = "(&(objectCategory=computer)(objectClass=computer))", // Filter for computers
                        PropertiesToLoad = { "dNSHostName" } // Load the DNS host name
                    };

                    foreach (SearchResult result in searcher.FindAll())
                    {
                        var hostname = result.Properties["dNSHostName"][0].ToString();
                        events.AddRange(GetLogonEventsForComputer(hostname, startTime, endTime));
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions appropriately (e.g., logging)
                    Console.WriteLine($"Error reading events from {domainController}: {ex.Message}");
                }
            }

            return events;
        }

        private List<EventLogEntry> GetLogonEventsForComputer(string computerName, DateTime startTime, DateTime endTime)
        {
            var events = new List<EventLogEntry>();
            var eventLog = new EventLog("Security", computerName);

            foreach (EventLogEntry entry in eventLog.Entries)
            {
                if (entry.TimeGenerated >= startTime && entry.TimeGenerated <= endTime &&
                    (entry.InstanceId == 4624 || entry.InstanceId == 4634) && // Logon/Logoff event IDs
                    entry.ReplacementStrings != null && entry.ReplacementStrings.Length > 5 &&
                    entry.ReplacementStrings[5] == "3") // Logon type 3 (Network)
                {
                    events.Add(entry);
                }
            }

            return events;
        }
        public List<EventLogEntry> GetUserLogonEvents(string userName, DateTime startTime, DateTime endTime)
        {
            var events = new List<EventLogEntry>();
            var dcNames = _directory.DomainControllers.Select(controller => controller.Name).ToList();
            foreach (var domainController in dcNames)
            {

                var result = _directory.Impersonation.Run(() =>
                 {
                     try
                     {
                         var eventLog = new EventLog("Security", domainController);

                         foreach (EventLogEntry entry in eventLog.Entries)
                         {
                             if (entry.TimeGenerated >= startTime && entry.TimeGenerated <= endTime &&
                                 (entry.InstanceId == 4624) && // Logon event ID
                                 entry.ReplacementStrings != null && entry.ReplacementStrings.Length > 1 &&
                                 entry.ReplacementStrings[1].Equals(userName, StringComparison.OrdinalIgnoreCase)) // Check username
                             {
                                 events.Add(entry);
                             }
                         }
                         return true;
                     }
                     catch (Exception ex)
                     {
                         // Handle exceptions appropriately (e.g., logging)
                         Console.WriteLine($"Error reading events from {domainController}: {ex.Message}");
                         return false;
                     }
                 });

            }

            return events;
        }
    }

}
