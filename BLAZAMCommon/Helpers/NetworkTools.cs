using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace BLAZAM.Helpers
{
    public class NetworkTools
    {

        public static bool PingHost(string hostNameOrAddress)
        {
            bool pingable = false;
            Ping pinger = new Ping();
            try
            {
                PingReply reply = pinger.Send(hostNameOrAddress, 1000, new byte[32]);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Ignore exception and return false
            }
            return pingable;
        }
        /// <summary>
        /// Checks if the following TCP port is currently open and reachable by the host machine
        /// </summary>
        /// <param name="hostNameOrAddress">The hostname, FQDN, or IP of the host to check</param>
        /// <param name="port">The port number to check</param>
        /// <returns>True if the port is open, otherwise false</returns>
        public static bool IsPortOpen(string hostNameOrAddress, int port)
        {
            return IsAnyPortOpen(hostNameOrAddress, new int[] { port });
        }
        public static bool IsAnyPortOpen(string hostNameOrAddress, int[] ports)
        {
            bool portOpen = false;
            IPAddress? ip;
            IPAddress.TryParse(hostNameOrAddress, out ip);

            foreach (int port in ports)
            {
                using (TcpClient client = new TcpClient())
                {
                    try
                    {
                        if (ip != null)
                            client.Connect(ip, port);
                        else
                            client.Connect(hostNameOrAddress, port);
                        portOpen = true;
                        break;
                    }
                    catch (SocketException)
                    {
                        // Ignore exception and try the next port or return false
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
