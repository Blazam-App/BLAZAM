using System;
using Xunit;
using BLAZAM.Helpers; // For NetworkTools
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks; // For Task.Delay in some tests if needed
using System.Linq;
using BLAZAM.Common.Exceptions;

namespace BLAZAMCommon.Tests.Helpers
{
    public class NetworkToolsTests
    {
        #region PingHost Tests
        [Fact]
        public void PingHost_LoopbackAddress_ReturnsTrue()
        {
            // This test might fail in highly restricted environments.
            Assert.True(NetworkTools.PingHost("127.0.0.1"));
        }

        [Fact]
        public void PingHost_NonExistentHost_ReturnsCorrectException()
        {
            // Using a TLD that is reserved for testing/documentation (RFC 2606 / RFC 6761)
            // or a clearly fake one.
            Assert.Throws<UnresolvableAddressException>(() =>
            {
                NetworkTools.PingHost("nonexistent-domain-for-blazam-tests.example.com");
            });
            Assert.Throws<UnresolvableAddressException>(() =>
            {
                NetworkTools.PingHost("another-unlikely-hostname-blazam.blazam");
            });
        }

        [Fact]
        public void PingHost_NullInput_ReturnsFalse()
        {
            Assert.False(NetworkTools.PingHost(null));
        }

        [Fact]
        public void PingHost_EmptyInput_ReturnsFalse()
        {
            Assert.False(NetworkTools.PingHost(""));
        }

        [Fact]
        public void PingHost_WhitespaceInput_ReturnsFalse()
        {
            Assert.False(NetworkTools.PingHost("   "));
        }
        #endregion PingHost Tests

        #region IsPortOpen Tests
        [Fact]
        public void IsPortOpen_UnusedPortOnLoopback_ReturnsFalse()
        {
            // Using a port in the dynamic/private range that's unlikely to be used.
            Assert.False(NetworkTools.IsPortOpen("127.0.0.1", 65534));
            Assert.False(NetworkTools.IsPortOpen("127.0.0.1", 59876)); // Another random high port
        }

        [Fact]
        public void IsPortOpen_NullHost_ReturnsFalse()
        {
            Assert.False(NetworkTools.IsPortOpen(null, 80));
        }

        [Fact]
        public void IsPortOpen_EmptyHost_ReturnsFalse()
        {
            Assert.False(NetworkTools.IsPortOpen("", 80));
        }

        [Fact]
        public void IsPortOpen_WhitespaceHost_ReturnsFalse()
        {
            Assert.False(NetworkTools.IsPortOpen("   ", 80));
        }

        [Theory]
        [InlineData(0)]       // IPEndPoint.MinPort is 0
        [InlineData(65536)]   // IPEndPoint.MaxPort is 65535
        public void IsPortOpen_InvalidPort_ThrowsArgumentOutOfRangeException(int invalidPort)
        {
            // As per documentation of TcpClient.Connect and observed behavior of NetworkTools,
            // an invalid port will cause ArgumentOutOfRangeException from client.Connect,
            // which is not caught by the SocketException handler in NetworkTools.
            Assert.Throws<ArgumentOutOfRangeException>(() => NetworkTools.IsPortOpen("127.0.0.1", invalidPort));
        }
        #endregion IsPortOpen Tests

        #region IsAnyPortOpen Tests
        [Fact]
        public void IsAnyPortOpen_UnusedPortsOnLoopback_ReturnsFalse()
        {
            Assert.False(NetworkTools.IsAnyPortOpen("127.0.0.1", new[] { 65534, 65533, 65532 }));
        }

        [Fact]
        public void IsAnyPortOpen_NullHost_ReturnsFalse()
        {
            Assert.False(NetworkTools.IsAnyPortOpen(null, new[] { 80, 443 }));
        }

        [Fact]
        public void IsAnyPortOpen_EmptyHost_ReturnsFalse()
        {
            Assert.False(NetworkTools.IsAnyPortOpen("", new[] { 80, 443 }));
        }

        [Fact]
        public void IsAnyPortOpen_WhitespaceHost_ReturnsFalse()
        {
            Assert.False(NetworkTools.IsAnyPortOpen("   ", new[] { 80, 443 }));
        }

        [Fact]
        public void IsAnyPortOpen_NullPortsArray_ReturnsFalse()
        {
            Assert.False(NetworkTools.IsAnyPortOpen("127.0.0.1", null));
        }

        [Fact]
        public void IsAnyPortOpen_EmptyPortsArray_ReturnsFalse()
        {
            Assert.False(NetworkTools.IsAnyPortOpen("127.0.0.1", new int[] { }));
        }

        [Theory]
        [InlineData(new[] { 65530, 0, 65531 })]  // Contains 0
        [InlineData(new[] { 65530, 65536, 65531 })] // Contains 65536
        public void IsAnyPortOpen_ArrayWithInvalidPort_ThrowsArgumentOutOfRangeException(int[] portsWithInvalid)
        {
            // If any port in the array is invalid, TcpClient.Connect will throw ArgumentOutOfRangeException.
            // This exception is not caught by the SocketException handler in NetworkTools.
            Assert.Throws<ArgumentOutOfRangeException>(() => NetworkTools.IsAnyPortOpen("127.0.0.1", portsWithInvalid));
        }

        [Fact]
        public void IsAnyPortOpen_ArrayWithOnlyValidButClosedPorts_ReturnsFalse()
        {
            Assert.False(NetworkTools.IsAnyPortOpen("127.0.0.1", new[] { 65500, 65001, 65002 }));
        }

        #endregion IsAnyPortOpen Tests

        #region Positive Port Open Tests
        private int GetAvailablePort()
        {
            // Get a random available port
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        [Fact]
        public void IsPortOpen_WhenPortIsActuallyOpen_ReturnsTrue()
        {
            int port = 0;
            TcpListener? listener = null;
            try
            {
                port = GetAvailablePort();
                listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();

                Assert.True(NetworkTools.IsPortOpen("127.0.0.1", port));
            }
            catch (SocketException ex) // Could happen if GetAvailablePort has race condition or other issue
            {
                // Skip test if we can't reliably bind the port
                Assert.True(false, $"Test skipped: Could not start TcpListener on port {port}. Error: {ex.Message}");
            }
            finally
            {
                listener?.Stop();
            }
        }

        [Fact]
        public void IsAnyPortOpen_WhenOnePortIsActuallyOpen_ReturnsTrue()
        {
            int openPort = 0;
            TcpListener? listener = null;
            var testPorts = new[] { 65501, 0, 65502 }; // Placeholder for openPort

            try
            {
                openPort = GetAvailablePort();
                testPorts[1] = openPort; // Set the actual open port

                listener = new TcpListener(IPAddress.Loopback, openPort);
                listener.Start();

                Assert.True(NetworkTools.IsAnyPortOpen("127.0.0.1", testPorts));
            }
            catch (SocketException ex)
            {
                Assert.True(false, $"Test skipped: Could not start TcpListener on port {openPort}. Error: {ex.Message}");
            }
            finally
            {
                listener?.Stop();
            }
        }

        [Fact]
        public void IsAnyPortOpen_WhenOnePortIsActuallyOpenAndOthersAreInvalid_ThrowsArgumentOutOfRangeException()
        {
            int openPort = 0;
            TcpListener? listener = null;
            // Array contains an invalid port (0) which should cause ArgumentOutOfRangeException
            // before the open port is successfully checked if the invalid one is hit first by iteration.
            // The iteration order of `int[]` is deterministic (index 0, 1, 2...).
            // So if port 0 is at index 0, it will throw. If openPort is at index 0, it will return true.
            // Let's test the throwing case by putting invalid port first.
            var portsWithInvalidFirst = new[] { 0, 0, 65502 }; // Placeholder for openPort at index 1

            try
            {
                openPort = GetAvailablePort();
                portsWithInvalidFirst[1] = openPort;

                listener = new TcpListener(IPAddress.Loopback, openPort);
                listener.Start(); // Start listener on a valid port

                // The method should throw because portsWithInvalidFirst[0] is 0.
                Assert.Throws<ArgumentOutOfRangeException>(() => NetworkTools.IsAnyPortOpen("127.0.0.1", portsWithInvalidFirst));
            }
            catch (SocketException ex)
            {
                Assert.True(false, $"Test skipped: Could not start TcpListener on port {openPort}. Error: {ex.Message}");
            }
            finally
            {
                listener?.Stop();
            }
        }

        #endregion Positive Port Open Tests

        #region DNS Resolution Tests
        /// <summary>
        /// Tests that when a valid IP address string is passed, the method
        /// correctly parses and returns the corresponding IPAddress object.
        /// This covers both IPv4 and IPv6 addresses.
        /// </summary>
        [Theory]
        [InlineData("127.0.0.1")]
        [InlineData("8.8.8.8")]
        [InlineData("::1")]
        [InlineData("2001:4860:4860::8888")]
        public void ResolveHostIP_WithValidIpAddress_ReturnsParsedIPAddress(string ipAddressString)
        {
            // Arrange
            var expectedIp = IPAddress.Parse(ipAddressString);

            // Act
            var result = NetworkTools.ResolveHostIP(ipAddressString);

            // Assert
            Assert.Equal(expectedIp, result);
        }

        /// <summary>
        /// Tests that a known, resolvable hostname like "localhost" returns a valid IPAddress.
        /// "localhost" is used because it resolves locally without needing an internet connection.
        /// </summary>
        [Fact]
        public void ResolveHostIP_WithValidHostname_ReturnsAnIPAddress()
        {
            // Arrange
            var hostName = "localhost";

            // Act
            var result = NetworkTools.ResolveHostIP(hostName);

            // Assert
            Assert.NotNull(result);
            // localhost can resolve to 127.0.0.1 (Loopback) or ::1 (IPv6Loopback)
            Assert.True(result.Equals(IPAddress.Loopback) || result.Equals(IPAddress.IPv6Loopback));
        }

        /// <summary>
        /// Tests that an unresolvable or invalid hostname causes the method
        /// to catch a SocketException and correctly return null.
        /// </summary>
        [Fact]
        public void ResolveHostIP_WithInvalidHostname_ReturnsNull()
        {
            // Arrange
            var invalidHostName = "this-is-not-a-valid-hostname.invalid";

            // Act
            var result = NetworkTools.ResolveHostIP(invalidHostName);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// This test verifies that passing a null input is not handled by the
        /// catch block and correctly throws an ArgumentNullException. This highlights
        /// an unhandled edge case in the current implementation.
        /// </summary>
        [Fact]
        public void ResolveHostIP_WithNullInput_ThrowsArgumentNullException()
        {
            // Arrange
            string? hostName = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => NetworkTools.ResolveHostIP(hostName!));
        }

        /// <summary>
        /// This test verifies that passing an empty string is not handled by the
        /// catch block and correctly throws an ArgumentException. This highlights
        /// another unhandled edge case.
        /// </summary>
        [Fact]
        public void ResolveHostIP_WithEmptyStringInput_ThrowsArgumentException()
        {
            // Arrange
            var hostName = string.Empty;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => NetworkTools.ResolveHostIP(hostName));
        }
        #endregion
    }
}
