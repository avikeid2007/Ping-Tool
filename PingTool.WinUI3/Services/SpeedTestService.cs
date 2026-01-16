using System.Diagnostics;
using System.Net.Http;

namespace PingTool.Services;

public class SpeedTestService
{
    // Use multiple test endpoints for reliability
    private static readonly string[] DownloadUrls = new[]
    {
        "https://speed.cloudflare.com/__down?bytes=10000000",  // 10MB from Cloudflare
        "https://proof.ovh.net/files/10Mb.dat",                 // 10MB backup
    };

    private static readonly string UploadUrl = "https://speed.cloudflare.com/__up";

    public event Action<double>? ProgressUpdated;
    public event Action<string>? StatusUpdated;

    public async Task<SpeedTestResult> RunSpeedTestAsync(CancellationToken cancellationToken = default)
    {
        var result = new SpeedTestResult();

        try
        {
            // Test Download Speed
            StatusUpdated?.Invoke("Testing download speed...");
            result.DownloadSpeedMbps = await TestDownloadSpeedAsync(cancellationToken);

            // Test Upload Speed
            StatusUpdated?.Invoke("Testing upload speed...");
            result.UploadSpeedMbps = await TestUploadSpeedAsync(cancellationToken);

            // Test Latency (ping to cloudflare)
            StatusUpdated?.Invoke("Testing latency...");
            result.LatencyMs = await TestLatencyAsync(cancellationToken);

            result.TestTime = DateTime.Now;
            result.IsSuccess = true;
            StatusUpdated?.Invoke("Test complete!");
        }
        catch (OperationCanceledException)
        {
            result.IsSuccess = false;
            result.Error = "Test cancelled";
            StatusUpdated?.Invoke("Test cancelled");
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Error = ex.Message;
            StatusUpdated?.Invoke($"Error: {ex.Message}");
        }

        return result;
    }

    private async Task<double> TestDownloadSpeedAsync(CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(30);

        foreach (var url in DownloadUrls)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                long totalBytes = 0;

                using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                var contentLength = response.Content.Headers.ContentLength ?? 10_000_000;
                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

                var buffer = new byte[81920];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    totalBytes += bytesRead;
                    var progress = (double)totalBytes / contentLength * 50; // 0-50%
                    ProgressUpdated?.Invoke(progress);
                }

                stopwatch.Stop();

                var seconds = stopwatch.Elapsed.TotalSeconds;
                var megabits = (totalBytes * 8.0) / 1_000_000;
                return Math.Round(megabits / seconds, 2);
            }
            catch
            {
                // Try next URL
                continue;
            }
        }

        return 0;
    }

    private async Task<double> TestUploadSpeedAsync(CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(30);

        try
        {
            // Create 2MB of test data
            var testData = new byte[2_000_000];
            new Random().NextBytes(testData);

            var stopwatch = Stopwatch.StartNew();

            using var content = new ByteArrayContent(testData);
            var response = await client.PostAsync(UploadUrl, content, cancellationToken);

            stopwatch.Stop();

            ProgressUpdated?.Invoke(75); // 50-75%

            var seconds = stopwatch.Elapsed.TotalSeconds;
            var megabits = (testData.Length * 8.0) / 1_000_000;
            return Math.Round(megabits / seconds, 2);
        }
        catch
        {
            return 0;
        }
    }

    private async Task<long> TestLatencyAsync(CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(10);

        var latencies = new List<long>();

        for (int i = 0; i < 5; i++)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                await client.GetAsync("https://1.1.1.1/cdn-cgi/trace", cancellationToken);
                stopwatch.Stop();
                latencies.Add(stopwatch.ElapsedMilliseconds);
                ProgressUpdated?.Invoke(75 + (i + 1) * 5); // 75-100%
            }
            catch
            {
                // Skip failed ping
            }
        }

        return latencies.Count > 0 ? (long)latencies.Average() : 0;
    }
}

public class SpeedTestResult
{
    public double DownloadSpeedMbps { get; set; }
    public double UploadSpeedMbps { get; set; }
    public long LatencyMs { get; set; }
    public DateTime TestTime { get; set; }
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }

    public string DownloadDisplay => $"{DownloadSpeedMbps:F1} Mbps";
    public string UploadDisplay => $"{UploadSpeedMbps:F1} Mbps";
    public string LatencyDisplay => $"{LatencyMs} ms";
}
