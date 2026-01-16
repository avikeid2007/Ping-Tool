using PingTool.Models;
using System.Net;
using System.Net.NetworkInformation;

namespace PingTool.Services;

public class TracerouteService
{
    private const int Timeout = 3000;
    private const int MaxHops = 30;
    private const int PingsPerHop = 3;

    public async IAsyncEnumerable<TracerouteHop> TraceAsync(string host, CancellationToken cancellationToken = default)
    {
        IPAddress? targetAddress;
        try
        {
            var hostEntry = await Dns.GetHostEntryAsync(host);
            targetAddress = hostEntry.AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            if (targetAddress == null)
            {
                yield break;
            }
        }
        catch
        {
            yield break;
        }

        using var ping = new Ping();
        var buffer = new byte[32];
        Array.Fill<byte>(buffer, 0x41);

        for (int ttl = 1; ttl <= MaxHops; ttl++)
        {
            if (cancellationToken.IsCancellationRequested) yield break;

            var hop = new TracerouteHop { HopNumber = ttl };
            var latencies = new List<long>();
            IPAddress? hopAddress = null;

            for (int i = 0; i < PingsPerHop; i++)
            {
                try
                {
                    var options = new PingOptions(ttl, true);
                    var reply = await ping.SendPingAsync(targetAddress, Timeout, buffer, options);

                    if (reply.Status == IPStatus.Success || reply.Status == IPStatus.TtlExpired)
                    {
                        hopAddress ??= reply.Address;
                        latencies.Add(reply.RoundtripTime);
                        hop.IsSuccess = true;
                    }
                    else if (reply.Status == IPStatus.TimedOut)
                    {
                        latencies.Add(-1);
                        hop.IsTimeout = latencies.All(l => l == -1);
                    }
                    else
                    {
                        latencies.Add(-1);
                    }
                }
                catch
                {
                    latencies.Add(-1);
                }
            }

            if (hopAddress != null)
            {
                hop.IpAddress = hopAddress.ToString();

                // Try to resolve hostname
                try
                {
                    var hostEntry = await Dns.GetHostEntryAsync(hopAddress);
                    hop.Hostname = hostEntry.HostName;
                }
                catch
                {
                    hop.Hostname = hop.IpAddress;
                }
            }
            else
            {
                hop.IpAddress = "*";
                hop.Hostname = "*";
                hop.IsTimeout = true;
            }

            hop.Latency1 = latencies.Count > 0 ? latencies[0] : -1;
            hop.Latency2 = latencies.Count > 1 ? latencies[1] : -1;
            hop.Latency3 = latencies.Count > 2 ? latencies[2] : -1;

            yield return hop;

            // Stop if we reached the destination
            if (hopAddress?.Equals(targetAddress) == true)
            {
                break;
            }
        }
    }
}
