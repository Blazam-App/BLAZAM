using BLAZAM.ActiveDirectory.Interfaces;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Reflection.Metadata;

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
        public List<EventRecord> GetUserLogonEvents(string userName, DateTime startTime, DateTime endTime)
        {
            
            var events = new List<EventRecord>();
            var dcNames = _directory.DomainControllers.Select(controller => controller.Name).ToList();
            Parallel.ForEach(dcNames, domainController => {
                var result = _directory.Impersonation.Run(() =>
                {
                    try
                    {
                        EventLogSession session = new EventLogSession(domainController);
                        var eventLogQuery = new EventLogQuery("Security", PathType.LogName, "*[System/EventID=4625] and *[EventData[Data[@Name='TargetUserName'] and (Data='" + userName + "')]]");
                        eventLogQuery.Session = session;

                        var reader = new EventLogReader(eventLogQuery);
                        for (EventRecord eventdetail = reader.ReadEvent(); eventdetail != null; eventdetail = reader.ReadEvent())
                        {
                            // Read Event details
                            events.Add(eventdetail);

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
            });


            return events;
        }
    }

}
