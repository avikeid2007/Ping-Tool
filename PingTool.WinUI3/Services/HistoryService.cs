using PingTool.Helpers;
using PingTool.Models;
using System.Text;

namespace PingTool.Services;

public class HistoryService
{
    // Singleton instance
    private static readonly Lazy<HistoryService> _instance = new(() => new HistoryService());
    public static HistoryService Instance => _instance.Value;

    private const int MaxHistoryItems = 500;
    private readonly List<HistoryItem> _history = new();
    private readonly object _lock = new();

    public event Action? HistoryChanged;

    public IReadOnlyList<HistoryItem> History => _history.AsReadOnly();

    private HistoryService()
    {
        LoadHistory();
    }

    public void AddHistory(HistoryItem item)
    {
        lock (_lock)
        {
            _history.Insert(0, item);

            // Trim to max items
            while (_history.Count > MaxHistoryItems)
            {
                _history.RemoveAt(_history.Count - 1);
            }

            SaveHistory();
            HistoryChanged?.Invoke();
        }
    }

    public void RemoveHistory(string id)
    {
        lock (_lock)
        {
            var item = _history.FirstOrDefault(h => h.Id == id);
            if (item != null)
            {
                _history.Remove(item);
                SaveHistory();
                HistoryChanged?.Invoke();
            }
        }
    }

    public void ClearHistory(HistoryType? type = null)
    {
        lock (_lock)
        {
            if (type.HasValue)
            {
                _history.RemoveAll(h => h.Type == type.Value);
            }
            else
            {
                _history.Clear();
            }
            SaveHistory();
            HistoryChanged?.Invoke();
        }
    }

    public IEnumerable<HistoryItem> GetHistory(HistoryType? type = null)
    {
        lock (_lock)
        {
            return type.HasValue
                ? _history.Where(h => h.Type == type.Value).ToList()
                : _history.ToList();
        }
    }

    public string ExportToText(HistoryItem item)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"=== {item.TypeDisplay} Report ===");
        sb.AppendLine($"Date: {item.Timestamp:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Target: {item.Target}");

        if (!string.IsNullOrEmpty(item.DomainName))
        {
            sb.AppendLine($"Domain: {item.DomainName}");
        }

        sb.AppendLine($"Status: {(item.IsSuccess ? "Success" : "Failed")}");
        sb.AppendLine();
        sb.AppendLine("--- Summary ---");
        sb.AppendLine(item.Summary);
        sb.AppendLine();

        switch (item.Type)
        {
            case HistoryType.Ping:
                sb.AppendLine("--- Statistics ---");
                sb.AppendLine($"Total Pings: {item.PingCount}");
                sb.AppendLine($"Average Latency: {item.AvgLatency}ms");
                sb.AppendLine($"Packet Loss: {item.PacketLoss:F1}%");
                break;

            case HistoryType.SpeedTest:
                sb.AppendLine("--- Speed Test Results ---");
                sb.AppendLine($"Download: {item.DownloadSpeed:F1} Mbps");
                sb.AppendLine($"Upload: {item.UploadSpeed:F1} Mbps");
                sb.AppendLine($"Latency: {item.Latency}ms");
                break;

            case HistoryType.PortScan:
                sb.AppendLine("--- Port Scan Results ---");
                sb.AppendLine($"Open Ports: {item.OpenPorts}");
                sb.AppendLine($"Closed Ports: {item.ClosedPorts}");
                sb.AppendLine($"Total Scanned: {item.TotalPorts}");
                break;

            case HistoryType.Traceroute:
                sb.AppendLine("--- Traceroute Results ---");
                sb.AppendLine($"Total Hops: {item.HopCount}");
                break;
        }

        if (!string.IsNullOrEmpty(item.Details))
        {
            sb.AppendLine();
            sb.AppendLine("--- Details ---");
            sb.AppendLine(item.Details);
        }

        return sb.ToString();
    }

    private void LoadHistory()
    {
        var saved = SettingsHelper.Read<List<HistoryItem>>("UnifiedHistory");
        if (saved != null)
        {
            _history.AddRange(saved);
        }
    }

    private void SaveHistory()
    {
        SettingsHelper.Save("UnifiedHistory", _history);
    }
}
