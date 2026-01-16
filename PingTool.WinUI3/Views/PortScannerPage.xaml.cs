using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using PingTool.Helpers;
using PingTool.Models;
using PingTool.Services;
using System.Collections.ObjectModel;
using System.Text;

namespace PingTool.Views;

public sealed partial class PortScannerPage : Page
{

    private readonly PortScannerService _scannerService = new();
    private CancellationTokenSource? _cts;
    private bool _isRunning;
    private bool _disclaimerAccepted;
    private string _currentHost = string.Empty;

    public ObservableCollection<PortScanResult> Results { get; } = new();

    public PortScannerPage()
    {
        InitializeComponent();

        LoadDisclaimer();
    }

    private void LoadDisclaimer()
    {
        _disclaimerAccepted = SettingsHelper.Read<bool?>("PortScanDisclaimerAccepted") ?? false;
        UpdateDisclaimerVisibility();
    }

    private void UpdateDisclaimerVisibility()
    {
        DisclaimerPanel.Visibility = _disclaimerAccepted ? Visibility.Collapsed : Visibility.Visible;
        ScannerPanel.Visibility = _disclaimerAccepted ? Visibility.Visible : Visibility.Collapsed;
    }

    private void AcceptDisclaimer_Click(object sender, RoutedEventArgs e)
    {
        _disclaimerAccepted = true;
        SettingsHelper.Save("PortScanDisclaimerAccepted", true);
        UpdateDisclaimerVisibility();
    }

    private async void ScanButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isRunning)
        {
            _cts?.Cancel();
            _isRunning = false;
            ScanButtonText.Text = "Scan";
            ScanButtonIcon.Glyph = "\uE768";
            ProgressBar.Visibility = Visibility.Collapsed;
            return;
        }

        var host = HostInput.Text?.Trim();
        if (string.IsNullOrEmpty(host)) return;

        var confirmed = await ShowAuthorizationDialogAsync(host);
        if (!confirmed) return;

        _currentHost = host;
        await ExecuteScanAsync(host);
    }

    private async Task<bool> ShowAuthorizationDialogAsync(string host)
    {
        var checkBox = new CheckBox
        {
            Content = "I confirm that I own this system or have permission to scan it.",
            Margin = new Thickness(0, 12, 0, 0)
        };

        var content = new StackPanel
        {
            Children =
            {
                new TextBlock
                {
                    Text = "You are about to perform a port scan on the target system.\n\n" +
                           "Port scanning should only be done on systems you own or have explicit permission to test. " +
                           "Unauthorized scanning may be illegal and could violate network or service provider policies.",
                    TextWrapping = TextWrapping.Wrap
                },
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 16, 0, 0),
                    Spacing = 8,
                    Children =
                    {
                        new TextBlock { Text = "Target:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold },
                        new TextBlock { Text = host, Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["AccentFillColorDefaultBrush"] }
                    }
                },
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 4, 0, 0),
                    Spacing = 8,
                    Children =
                    {
                        new TextBlock { Text = "Scan Type:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold },
                        new TextBlock { Text = "Port Scan" }
                    }
                },
                checkBox
            }
        };

        var dialog = new ContentDialog
        {
            Title = "Port Scan Authorization",
            Content = content,
            PrimaryButtonText = "Start Scan",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot,
            IsPrimaryButtonEnabled = false
        };

        checkBox.Checked += (s, e) => dialog.IsPrimaryButtonEnabled = true;
        checkBox.Unchecked += (s, e) => dialog.IsPrimaryButtonEnabled = false;

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    private async Task ExecuteScanAsync(string host)
    {
        Results.Clear();
        _isRunning = true;
        ScanButtonText.Text = "Stop";
        ScanButtonIcon.Glyph = "\uE711";
        ProgressBar.Visibility = Visibility.Visible;
        ProgressBar.Value = 0;
        EmptyState.Visibility = Visibility.Collapsed;
        StatsPanel.Visibility = Visibility.Visible;
        _cts = new CancellationTokenSource();

        int open = 0, closed = 0, filtered = 0;
        var ports = PortScannerService.CommonPorts;
        int current = 0;

        try
        {
            await foreach (var result in _scannerService.ScanPortsAsync(host, ports, cancellationToken: _cts.Token))
            {
                Results.Add(result);
                current++;
                ProgressBar.Value = (current / (double)ports.Length) * 100;

                if (result.IsOpen)
                    open++;
                else if (result.Status.Contains("Filtered"))
                    filtered++;
                else
                    closed++;

                OpenCount.Text = open.ToString();
                ClosedCount.Text = closed.ToString();
                FilteredCount.Text = filtered.ToString();
            }

            // Save to history after scan completes
            if (Results.Count > 0)
            {
                SaveToHistory(host, open, closed, filtered);
            }
        }
        catch (OperationCanceledException)
        {
            // Cancelled
        }
        finally
        {
            _isRunning = false;
            ScanButtonText.Text = "Scan";
            ScanButtonIcon.Glyph = "\uE768";
            ProgressBar.Visibility = Visibility.Collapsed;
            if (Results.Count == 0)
            {
                EmptyState.Visibility = Visibility.Visible;
                StatsPanel.Visibility = Visibility.Collapsed;
            }
        }
    }

    private void SaveToHistory(string host, int open, int closed, int filtered)
    {
        var sb = new StringBuilder();
        foreach (var result in Results)
        {
            sb.AppendLine($"Port {result.Port} ({result.ServiceName}): {result.Status}");
        }

        var historyItem = new HistoryItem
        {
            Type = HistoryType.PortScan,
            Target = host,
            IsSuccess = open > 0,
            OpenPorts = open,
            ClosedPorts = closed + filtered,
            TotalPorts = Results.Count,
            Summary = $"{open} open, {closed} closed, {filtered} filtered ports",
            Details = sb.ToString()
        };

        HistoryService.Instance.AddHistory(historyItem);
    }

    private void HostInput_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && !_isRunning && _disclaimerAccepted)
        {
            ScanButton_Click(sender, e);
        }
    }
}
