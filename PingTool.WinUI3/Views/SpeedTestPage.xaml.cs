using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PingTool.Models;
using PingTool.Services;

namespace PingTool.Views;

public sealed partial class SpeedTestPage : Page
{

    private readonly SpeedTestService _speedTestService = new();
    private CancellationTokenSource? _cts;
    private bool _isRunning;

    public SpeedTestPage()
    {
        InitializeComponent();

        _speedTestService.ProgressUpdated += OnProgressUpdated;
        _speedTestService.StatusUpdated += OnStatusUpdated;
    }

    private void OnProgressUpdated(double progress)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            ProgressBar.Value = progress;
        });
    }

    private void OnStatusUpdated(string status)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            StatusText.Text = status;
        });
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isRunning)
        {
            _cts?.Cancel();
            _isRunning = false;
            StartButtonText.Text = "Start Test";
            StartButtonIcon.Glyph = "\uE768";
            ProgressBar.Visibility = Visibility.Collapsed;
            return;
        }

        _isRunning = true;
        StartButtonText.Text = "Stop";
        StartButtonIcon.Glyph = "\uE711";
        ProgressBar.Visibility = Visibility.Visible;
        ProgressBar.Value = 0;

        // Reset displays
        DownloadSpeed.Text = "--";
        UploadSpeed.Text = "--";
        Latency.Text = "-- ms";

        _cts = new CancellationTokenSource();

        try
        {
            var result = await _speedTestService.RunSpeedTestAsync(_cts.Token);

            if (result.IsSuccess)
            {
                DownloadSpeed.Text = result.DownloadSpeedMbps.ToString("F1");
                UploadSpeed.Text = result.UploadSpeedMbps.ToString("F1");
                Latency.Text = $"{result.LatencyMs} ms";
                StatusText.Text = $"Test completed at {result.TestTime:HH:mm:ss}";

                // Save to history
                SaveToHistory(result);
            }
            else
            {
                StatusText.Text = result.Error ?? "Test failed";
            }
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "Test cancelled";
        }
        finally
        {
            _isRunning = false;
            StartButtonText.Text = "Start Test";
            StartButtonIcon.Glyph = "\uE768";
            ProgressBar.Visibility = Visibility.Collapsed;
        }
    }

    private void SaveToHistory(SpeedTestResult result)
    {
        var historyItem = new HistoryItem
        {
            Type = HistoryType.SpeedTest,
            Target = "Internet Speed Test",
            IsSuccess = result.IsSuccess,
            DownloadSpeed = result.DownloadSpeedMbps,
            UploadSpeed = result.UploadSpeedMbps,
            Latency = result.LatencyMs,
            Summary = $"Download: {result.DownloadSpeedMbps:F1} Mbps, Upload: {result.UploadSpeedMbps:F1} Mbps, Latency: {result.LatencyMs}ms",
            Details = $"Test completed at {result.TestTime:HH:mm:ss}\n" +
                     $"Download Speed: {result.DownloadSpeedMbps:F1} Mbps\n" +
                     $"Upload Speed: {result.UploadSpeedMbps:F1} Mbps\n" +
                     $"Latency: {result.LatencyMs}ms"
        };

        HistoryService.Instance.AddHistory(historyItem);
    }
}
