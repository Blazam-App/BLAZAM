using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using BLAZAM.Common.Data.Validators;

namespace BLAZAM.Common.Helpers
{
    /// <summary>
    /// Provides utility methods for network-related operations like pinging hosts and checking port statuses.
    /// </summary>
    public static class NetworkTools
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

            IPAddress? ip = TryResolveHostIP(hostNameOrAddress);
            if (ip == null) return false;

            Ping pinger = new();
            try
            {
                PingReply reply = pinger.Send(ip, 1000, new byte[32]);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
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


            IPAddress? ip = TryResolveHostIP(hostNameOrAddress);
            if (ip == null) return false;

            foreach (int port in ports)
            {
                if (port < 1 || port > 65535)
                {
                    throw new ArgumentOutOfRangeException(nameof(ports), "Ports must be between 1-65535");
                }
                using (TcpClient client = new())
                {
                    try
                    {

                        client.Connect(ip, port);
                        portOpen = true; // Port is open
                        break; // Exit loop since one open port is found
                    }
                    catch (SocketException)
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

        public static IPAddress? TryResolveHostIP(string hostNameOrAddress)
        {
            try
            {
                return ResolveHostIP(hostNameOrAddress);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static IPAddress? ResolveHostIP(string hostNameOrAddress)
        {
            if (hostNameOrAddress == null) throw new ArgumentNullException(nameof(hostNameOrAddress));
            if (hostNameOrAddress == string.Empty) throw new ArgumentException(nameof(hostNameOrAddress));
            IPAddress? ip;
            var validator = new ValidIpAttribute();
            if (validator.IsValid(hostNameOrAddress)) return IPAddress.Parse(hostNameOrAddress);
            try
            {
                IPAddress[] addresses = Dns.GetHostAddresses(hostNameOrAddress);

                // Return the first address found (often the IPv4 address).
                return addresses.FirstOrDefault();
            }
            catch (SocketException ex) when (ex.ErrorCode == 11001)
            {
                throw new UnresolvableAddressException(hostNameOrAddress);
            }
        }
    }
}
