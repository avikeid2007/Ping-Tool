using System.Net;
using System.Net.Sockets;

namespace PingTool.Services;

public class DnsLookupService
{
    public async Task<DnsLookupResult> LookupAsync(string hostname)
    {
        var result = new DnsLookupResult { Hostname = hostname };

        try
        {
            var hostEntry = await Dns.GetHostEntryAsync(hostname);

            result.CanonicalName = hostEntry.HostName;
            result.Aliases = hostEntry.Aliases.ToList();

            foreach (var ip in hostEntry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    result.IPv4Addresses.Add(ip.ToString());
                }
                else if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    result.IPv6Addresses.Add(ip.ToString());
                }
            }

            result.IsSuccess = true;
        }
        catch (SocketException ex)
        {
            result.Error = ex.Message;
            result.IsSuccess = false;
        }
        catch (Exception ex)
        {
            result.Error = ex.Message;
            result.IsSuccess = false;
        }

        return result;
    }
}

public class DnsLookupResult
{
    public string Hostname { get; set; } = string.Empty;
    public string CanonicalName { get; set; } = string.Empty;
    public List<string> IPv4Addresses { get; set; } = new();
    public List<string> IPv6Addresses { get; set; } = new();
    public List<string> Aliases { get; set; } = new();
    public bool IsSuccess { get; set; }
    public string Error { get; set; } = string.Empty;
}
