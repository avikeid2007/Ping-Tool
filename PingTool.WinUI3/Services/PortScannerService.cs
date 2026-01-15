using System.Net.Sockets;

namespace PingTool.Services;

public class PortScannerService
{
    private const int DefaultTimeout = 1000; // 1 second timeout

    /// <summary>
    /// LEGAL NOTICE: Port scanning should only be performed on systems you own or have explicit 
    /// authorization to scan. Unauthorized port scanning may violate computer crime laws in many 
    /// jurisdictions. The user accepts full responsibility for the use of this feature.
    /// </summary>
    public async Task<PortScanResult> ScanPortAsync(string host, int port, int timeout = DefaultTimeout)
    {
        var result = new PortScanResult
        {
            Host = host,
            Port = port,
            ServiceName = GetCommonServiceName(port)
        };

        try
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(host, port);
            
            if (await Task.WhenAny(connectTask, Task.Delay(timeout)) == connectTask)
            {
                if (client.Connected)
                {
                    result.IsOpen = true;
                    result.Status = "Open";
                }
                else
                {
                    result.IsOpen = false;
                    result.Status = "Closed";
                }
            }
            else
            {
                result.IsOpen = false;
                result.Status = "Filtered/Timeout";
            }
        }
        catch (SocketException ex)
        {
            result.IsOpen = false;
            result.Status = ex.SocketErrorCode switch
            {
                SocketError.ConnectionRefused => "Closed",
                SocketError.TimedOut => "Filtered",
                SocketError.HostUnreachable => "Unreachable",
                SocketError.NetworkUnreachable => "Network Unreachable",
                _ => $"Error: {ex.SocketErrorCode}"
            };
        }
        catch (Exception ex)
        {
            result.IsOpen = false;
            result.Status = $"Error: {ex.Message}";
        }

        return result;
    }

    public async IAsyncEnumerable<PortScanResult> ScanPortsAsync(
        string host, 
        IEnumerable<int> ports, 
        int timeout = DefaultTimeout,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var port in ports)
        {
            if (cancellationToken.IsCancellationRequested) yield break;
            yield return await ScanPortAsync(host, port, timeout);
        }
    }

    public static readonly int[] CommonPorts = new[]
    {
        21,   // FTP
        22,   // SSH
        23,   // Telnet
        25,   // SMTP
        53,   // DNS
        80,   // HTTP
        110,  // POP3
        143,  // IMAP
        443,  // HTTPS
        445,  // SMB
        993,  // IMAPS
        995,  // POP3S
        1433, // MSSQL
        1521, // Oracle
        3306, // MySQL
        3389, // RDP
        5432, // PostgreSQL
        5900, // VNC
        6379, // Redis
        8080, // HTTP Alternate
        8443, // HTTPS Alternate
        27017 // MongoDB
    };

    public static readonly int[] WebPorts = new[] { 80, 443, 8080, 8443, 3000, 5000, 8000 };

    private static string GetCommonServiceName(int port) => port switch
    {
        21 => "FTP",
        22 => "SSH",
        23 => "Telnet",
        25 => "SMTP",
        53 => "DNS",
        80 => "HTTP",
        110 => "POP3",
        143 => "IMAP",
        443 => "HTTPS",
        445 => "SMB",
        993 => "IMAPS",
        995 => "POP3S",
        1433 => "MSSQL",
        1521 => "Oracle",
        3306 => "MySQL",
        3389 => "RDP",
        5432 => "PostgreSQL",
        5900 => "VNC",
        6379 => "Redis",
        8080 => "HTTP Alt",
        8443 => "HTTPS Alt",
        27017 => "MongoDB",
        _ => ""
    };
}

public class PortScanResult
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool IsOpen { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Display => $"{Port} ({ServiceName})".TrimEnd('(', ')', ' ');
}
