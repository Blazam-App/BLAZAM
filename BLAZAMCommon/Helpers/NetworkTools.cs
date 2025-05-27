using BLAZAM.Logger; // Added
using System; // Added
using System.Linq; // Added for ports.Length
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace BLAZAM.Helpers
{
    /// <summary>
    /// Provides utility methods for network-related operations like pinging hosts and checking port statuses.
    /// </summary>
    public class NetworkTools
    {
        /// <summary>
        /// Attempts a single ping request.
        /// </summary>
        /// <param name="hostNameOrAddress">The destination to ping.</param>
        /// <returns>True on a successful ping response; otherwise, false (including if hostNameOrAddress is null/empty or a PingException occurs).</returns>
        public static bool PingHost(string hostNameOrAddress)
        {
            if (string.IsNullOrWhiteSpace(hostNameOrAddress))
            {
                return false;
            }
            bool pingable = false;
            Ping pinger = new();
            try
            {
                PingReply reply = pinger.Send(hostNameOrAddress, 1000, new byte[32]);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException ex)
            {
                // PingException is caught, and pingable remains false.
            }
            return pingable;
        }

        /// <summary>
        /// Checks if the following TCP port is currently open and reachable by the host machine.
        /// </summary>
        /// <param name="hostNameOrAddress">The hostname, FQDN, or IP of the host to check.</param>
        /// <param name="port">The port number to check.</param>
        /// <returns>True if the port is open; otherwise, false (including if hostNameOrAddress or port is invalid, or a SocketException occurs).</returns>
        public static bool IsPortOpen(string hostNameOrAddress, int port)
        {
            return IsAnyPortOpen(hostNameOrAddress, new int[] { port });
        }

        /// <summary>
        /// Checks if any of the provided TCP ports is currently open and reachable by the host machine.
        /// </summary>
        /// <param name="hostNameOrAddress">The hostname, FQDN, or IP of the host to check.</param>
        /// <param name="ports">The port numbers to check.</param>
        /// <returns>True if any of the specified ports are open; otherwise, false (including if hostNameOrAddress or ports array is invalid/empty, or a SocketException occurs).</returns>
        public static bool IsAnyPortOpen(string hostNameOrAddress, int[] ports)
        {
            if (string.IsNullOrWhiteSpace(hostNameOrAddress))
            {
                return false;
            }
            if (ports == null || ports.Length == 0)
            {
                return false;
            }

            bool portOpen = false;
            IPAddress? ip;
            IPAddress.TryParse(hostNameOrAddress, out ip);

            foreach (int port in ports)
            {
                if(port < 1 || port > 65535)
                {
                    throw new ArgumentOutOfRangeException("Ports must be between 1-65535");
                }
                using (TcpClient client = new())
                {
                    try
                    {
                        if (ip != null)
                            client.Connect(ip, port);
                        else
                            client.Connect(hostNameOrAddress, port);
                        portOpen = true; // Port is open
                        break; // Exit loop since one open port is found
                    }
                    catch (SocketException ex)
                    {
                        // SocketException is caught, portOpen remains false for this port, loop continues.
                    }
                    finally
                    {
                        client.Close();
                    }
                }
            }
            return portOpen;
        }
    }
}
