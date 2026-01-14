using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;

namespace PingTool.Services;

/// <summary>
/// Native ICMP ping service - no more Desktop Bridge workaround needed in WinAppSDK!
/// </summary>
public class PingService : IDisposable
{
    private readonly Ping _pingSender = new();
    private CancellationTokenSource? _cts;

    /// <summary>
    /// Starts continuous ping to the specified host.
    /// </summary>
    public async IAsyncEnumerable<PingResult> StartPingAsync(
        string hostNameOrAddress,
        int delayMs = 800,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            PingResult result;
            try
            {
                var reply = await _pingSender.SendPingAsync(hostNameOrAddress, 4000);
                
                if (reply.Status == IPStatus.Success)
                {
                    result = new PingResult
                    {
                        IpAddress = reply.Address?.ToString() ?? hostNameOrAddress,
                        Time = reply.RoundtripTime,
                        Size = reply.Buffer?.Length ?? 0,
                        Ttl = reply.Options?.Ttl ?? 0,
                        Response = $"Reply from {reply.Address}: bytes={reply.Buffer?.Length ?? 0} time={reply.RoundtripTime}ms TTL={reply.Options?.Ttl ?? 0}",
                        IsSuccess = true
                    };
                }
                else
                {
                    result = new PingResult
                    {
                        IpAddress = hostNameOrAddress,
                        Time = -1,
                        Size = 0,
                        Ttl = 0,
                        Response = GetStatusMessage(reply.Status),
                        IsSuccess = false
                    };
                }
            }
            catch (Exception ex)
            {
                result = new PingResult
                {
                    IpAddress = hostNameOrAddress,
                    Time = -1,
                    Size = 0,
                    Ttl = 0,
                    Response = $"Ping failed: {ex.Message}",
                    IsSuccess = false
                };
            }

            yield return result;

            try
            {
                await Task.Delay(delayMs, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                yield break;
            }
        }
    }

    /// <summary>
    /// Creates a cancellation token for stopping ping.
    /// </summary>
    public CancellationToken GetCancellationToken()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        return _cts.Token;
    }

    /// <summary>
    /// Stops the current ping operation.
    /// </summary>
    public void Stop()
    {
        _cts?.Cancel();
    }

    private static string GetStatusMessage(IPStatus status) => status switch
    {
        IPStatus.TimedOut => "Request timed out.",
        IPStatus.DestinationNetworkUnreachable => "Destination Network Unreachable.",
        IPStatus.DestinationHostUnreachable => "Destination Host Unreachable.",
        IPStatus.DestinationProhibited => "Destination Prohibited.",
        IPStatus.DestinationPortUnreachable => "Destination Port Unreachable.",
        IPStatus.NoResources => "No Resources.",
        IPStatus.BadOption => "Bad Option.",
        IPStatus.HardwareError => "Hardware Error.",
        IPStatus.PacketTooBig => "Packet Too Big.",
        IPStatus.BadRoute => "Bad Route.",
        IPStatus.TtlExpired => "TTL Expired.",
        IPStatus.TtlReassemblyTimeExceeded => "TTL Reassembly Time Exceeded.",
        IPStatus.ParameterProblem => "Parameter Problem.",
        IPStatus.SourceQuench => "Source Quench.",
        IPStatus.BadDestination => "Bad Destination.",
        IPStatus.DestinationUnreachable => "Destination Unreachable.",
        IPStatus.TimeExceeded => "Time Exceeded.",
        IPStatus.BadHeader => "Bad Header.",
        IPStatus.UnrecognizedNextHeader => "Unrecognized Next Header.",
        IPStatus.IcmpError => "ICMP Error.",
        IPStatus.DestinationScopeMismatch => "Destination Scope Mismatch.",
        _ => "Request timed out."
    };

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _pingSender.Dispose();
    }
}

/// <summary>
/// Result of a ping operation.
/// </summary>
public class PingResult
{
    public string IpAddress { get; set; } = string.Empty;
    public long Time { get; set; }
    public int Size { get; set; }
    public int Ttl { get; set; }
    public string Response { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
}
