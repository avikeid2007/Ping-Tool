namespace PingTool.Models;

public class TracerouteHop
{
    public int HopNumber { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public long Latency1 { get; set; }
    public long Latency2 { get; set; }
    public long Latency3 { get; set; }
    public bool IsSuccess { get; set; }
    public bool IsTimeout { get; set; }
    public string Status => IsTimeout ? "Timeout" : (IsSuccess ? "OK" : "Failed");
    public string LatencyDisplay => IsTimeout ? "* * *" : $"{Latency1}ms {Latency2}ms {Latency3}ms";
}
