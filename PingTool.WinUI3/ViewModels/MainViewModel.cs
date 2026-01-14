using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Input;
using PingTool.Helpers;
using PingTool.Models;
using PingTool.Services;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using Windows.Networking.Connectivity;
using Windows.System;

namespace PingTool.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly PingService _pingService = new();
    private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

    private CancellationTokenSource? _pingCts;
    private Guid _pingId;
    private DateTimeOffset _pingDate;

    private readonly string _urlRegex = @"^(http|https|ftp|)?\://|[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";
    private readonly string _ipRegex = @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}";

    // Ping statistics tracking
    private readonly List<long> _successfulPingTimes = new();
    private int _totalPings;
    private int _failedPings;

    #region Network Properties

    [ObservableProperty]
    private string _ipAddress = string.Empty;

    [ObservableProperty]
    private string _profileName = string.Empty;

    [ObservableProperty]
    private bool _isWlan;

    [ObservableProperty]
    private long _totalReceivedBytes;

    [ObservableProperty]
    private long _totalSentBytes;

    [ObservableProperty]
    private bool _isSupportIPV6;

    [ObservableProperty]
    private int _wifiBars;

    [ObservableProperty]
    private string _hostNameOrAddress = "8.8.8.8";

    [ObservableProperty]
    private string _ipType = string.Empty;

    [ObservableProperty]
    private ObservableCollection<NetworkInterface> _localLANCollection = new();

    [ObservableProperty]
    private NetworkInterface? _selectedLocalLAN;

    [ObservableProperty]
    private bool _hasInternetAccess;

    [ObservableProperty]
    private bool _isPingStarted;

    [ObservableProperty]
    private bool _isCompactMode;

    [ObservableProperty]
    private ObservableCollection<PingMassage> _pingCollection = new();

    #endregion

    #region Statistics Properties

    [ObservableProperty]
    private long _minPing;

    [ObservableProperty]
    private long _maxPing;

    [ObservableProperty]
    private double _avgPing;

    [ObservableProperty]
    private double _packetLoss;

    [ObservableProperty]
    private int _successCount;

    [ObservableProperty]
    private int _failCount;

    #endregion

    #region Favorites Properties

    [ObservableProperty]
    private ObservableCollection<string> _favoriteHosts = new();

    [ObservableProperty]
    private string? _selectedFavorite;

    #endregion

    #region Chart Data

    [ObservableProperty]
    private ObservableCollection<double> _chartValues = new();

    private const int MaxChartPoints = 50;

    #endregion

    public async Task InitializeAsync()
    {
        // Load saved host address
        var savedAddress = SettingsHelper.Read<string>("HostNameOrAddress");
        if (!string.IsNullOrEmpty(savedAddress))
        {
            HostNameOrAddress = savedAddress;
        }

        // Load favorites
        LoadFavorites();

        // Load auto-start preference
        var isPingAutoStart = SettingsHelper.Read<bool?>("IsPingAutoStart") ?? true;

        await SetNetworkInfoAsync();
        NetworkInformation.NetworkStatusChanged += async _ => await SetNetworkInfoAsync();

        if (isPingAutoStart)
        {
            await Task.Delay(2000);
            await StartPingAsync();
        }
    }

    #region Ping Commands

    [RelayCommand]
    private async Task StartPingAsync()
    {
        if (!IsPingStarted)
        {
            if (IsValidHostNameOrAddress(HostNameOrAddress))
            {
                _pingId = Guid.NewGuid();
                _pingDate = DateTimeOffset.Now;
                IsPingStarted = true;

                // Reset statistics
                ResetStatistics();

                // Save the host address
                SettingsHelper.Save("HostNameOrAddress", HostNameOrAddress);

                // Start ping with cancellation support
                _pingCts = new CancellationTokenSource();
                var cleanHost = HostNameOrAddress.Replace("http://", "").Replace("https://", "").TrimEnd('/');

                _ = Task.Run(async () =>
                {
                    await foreach (var result in _pingService.StartPingAsync(cleanHost, cancellationToken: _pingCts.Token))
                    {
                        var pingMessage = new PingMassage
                        {
                            PingId = _pingId,
                            Date = _pingDate,
                            IpAddress = result.IpAddress,
                            Time = result.Time,
                            Size = result.Size,
                            Ttl = result.Ttl,
                            Response = result.Response
                        };

                        _dispatcherQueue.TryEnqueue(() =>
                        {
                            PingCollection.Add(pingMessage);
                            UpdateStatistics(result);
                            SQLiteHelper.Save(pingMessage);
                        });
                    }
                });

                await DeleteOlderHistoryAsync();
            }
        }
        else
        {
            // Stop ping
            _pingCts?.Cancel();
            _pingService.Stop();
            IsPingStarted = false;
        }
    }

    [RelayCommand]
    private void ClearPing()
    {
        PingCollection.Clear();
        ChartValues.Clear();
        ResetStatistics();
    }

    [RelayCommand]
    private void CopyIpAddress()
    {
        FileHelper.CopyText(IpAddress);
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        if (PingCollection.Count > 0)
        {
            var stats = $"=== Ping Statistics ===\r\n" +
                       $"Host: {HostNameOrAddress}\r\n" +
                       $"Min: {MinPing}ms | Max: {MaxPing}ms | Avg: {AvgPing:F1}ms\r\n" +
                       $"Packet Loss: {PacketLoss:F1}% ({FailCount}/{_totalPings})\r\n" +
                       $"========================\r\n\r\n";
            var text = stats + string.Join("\r\n", PingCollection.Select(x => x.Response));
            await FileHelper.SaveFileAsync(text, "ping.txt");
        }
    }

    [RelayCommand]
    private async Task ToggleCompactModeAsync()
    {
        IsCompactMode = !IsCompactMode;
        if (IsCompactMode && !IsPingStarted)
        {
            await StartPingAsync();
        }
    }

    #endregion

    #region Favorites Commands

    [RelayCommand]
    private void AddToFavorites()
    {
        if (!string.IsNullOrWhiteSpace(HostNameOrAddress) && !FavoriteHosts.Contains(HostNameOrAddress))
        {
            FavoriteHosts.Add(HostNameOrAddress);
            SaveFavorites();
        }
    }

    [RelayCommand]
    private void RemoveFromFavorites(string host)
    {
        if (FavoriteHosts.Contains(host))
        {
            FavoriteHosts.Remove(host);
            SaveFavorites();
        }
    }

    [RelayCommand]
    private async Task SelectFavoriteAsync(string host)
    {
        if (!string.IsNullOrWhiteSpace(host))
        {
            // Stop current ping if running
            if (IsPingStarted)
            {
                await StartPingAsync();
            }

            HostNameOrAddress = host;
            PingCollection.Clear();
            ChartValues.Clear();
            ResetStatistics();

            // Start pinging the new host
            await StartPingAsync();
        }
    }

    private void LoadFavorites()
    {
        var favorites = SettingsHelper.Read<List<string>>("FavoriteHosts");
        if (favorites != null)
        {
            FavoriteHosts = new ObservableCollection<string>(favorites);
        }
        else
        {
            // Add some default favorites
            FavoriteHosts = new ObservableCollection<string>
            {
                "8.8.8.8",
                "1.1.1.1",
                "google.com",
                "cloudflare.com"
            };
            SaveFavorites();
        }
    }

    private void SaveFavorites()
    {
        SettingsHelper.Save("FavoriteHosts", FavoriteHosts.ToList());
    }

    #endregion

    #region Keyboard Shortcuts

    public async Task HandleKeyboardShortcutAsync(VirtualKey key, bool isCtrlPressed)
    {
        switch (key)
        {
            case VirtualKey.F5:
                if (!IsPingStarted)
                    await StartPingAsync();
                break;
            case VirtualKey.Escape:
                if (IsPingStarted)
                    await StartPingAsync(); // Toggle off
                break;
            case VirtualKey.E when isCtrlPressed:
                await ExportAsync();
                break;
            case VirtualKey.D when isCtrlPressed:
                ClearPing();
                break;
            case VirtualKey.F when isCtrlPressed:
                AddToFavorites();
                break;
        }
    }

    #endregion

    #region Statistics Methods

    private void ResetStatistics()
    {
        _successfulPingTimes.Clear();
        _totalPings = 0;
        _failedPings = 0;
        MinPing = 0;
        MaxPing = 0;
        AvgPing = 0;
        PacketLoss = 0;
        SuccessCount = 0;
        FailCount = 0;
    }

    private void UpdateStatistics(PingResult result)
    {
        _totalPings++;

        if (result.IsSuccess && result.Time >= 0)
        {
            _successfulPingTimes.Add(result.Time);
            SuccessCount++;

            MinPing = _successfulPingTimes.Min();
            MaxPing = _successfulPingTimes.Max();
            AvgPing = _successfulPingTimes.Average();

            // Update chart
            ChartValues.Add(result.Time);
            if (ChartValues.Count > MaxChartPoints)
            {
                ChartValues.RemoveAt(0);
            }
        }
        else
        {
            _failedPings++;
            FailCount++;

            // Add a spike value for failed pings in chart
            ChartValues.Add(-1);
            if (ChartValues.Count > MaxChartPoints)
            {
                ChartValues.RemoveAt(0);
            }
        }

        PacketLoss = _totalPings > 0 ? (double)_failedPings / _totalPings * 100 : 0;
    }

    #endregion

    #region Network Methods

    private async Task SetNetworkInfoAsync()
    {
        try
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            _dispatcherQueue.TryEnqueue(() =>
            {
                LocalLANCollection = new ObservableCollection<NetworkInterface>(NetworkInterface.GetAllNetworkInterfaces());
            });

            if (icp?.NetworkAdapter == null)
            {
                _dispatcherQueue.TryEnqueue(ClearNetworkStatus);
                return;
            }

            var hasInternet = icp.GetNetworkConnectivityLevel() >= NetworkConnectivityLevel.InternetAccess;
            var hostname = NetworkInformation.GetHostNames()
                .FirstOrDefault(hn => hn.IPInformation?.NetworkAdapter != null
                    && hn.IPInformation.NetworkAdapter.NetworkAdapterId == icp.NetworkAdapter.NetworkAdapterId);

            if (hostname != null)
            {
                var currentNetworkId = "{" + hostname.IPInformation!.NetworkAdapter.NetworkAdapterId.ToString().ToUpper() + "}";

                _dispatcherQueue.TryEnqueue(() =>
                {
                    HasInternetAccess = hasInternet;
                    SelectedLocalLAN = LocalLANCollection.FirstOrDefault(x => x.Id.ToUpper() == currentNetworkId);
                    IpAddress = hostname.CanonicalName;
                    IpType = hostname.Type.ToString();
                    ProfileName = icp.ProfileName;
                    IsWlan = icp.IsWlanConnectionProfile;
                    WifiBars = Convert.ToInt32(icp.GetSignalBars());

                    if (SelectedLocalLAN != null)
                    {
                        var ipStats = SelectedLocalLAN.GetIPStatistics();
                        TotalReceivedBytes = ipStats.BytesReceived / 1048576;
                        TotalSentBytes = ipStats.BytesSent / 1048576;
                        IsSupportIPV6 = SelectedLocalLAN.Supports(NetworkInterfaceComponent.IPv6);
                    }
                });
            }
        }
        catch
        {
            // Ignore network info errors
        }
    }

    private void ClearNetworkStatus()
    {
        IpAddress = string.Empty;
        IpType = string.Empty;
        ProfileName = string.Empty;
        HasInternetAccess = false;
    }

    private async Task DeleteOlderHistoryAsync()
    {
        try
        {
            await Task.Run(() => SQLiteHelper.DeleteOld(9));
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    private bool IsValidHostNameOrAddress(string hostNameOrAddress)
    {
        if (string.IsNullOrEmpty(hostNameOrAddress)) return false;
        return IsValidHost(hostNameOrAddress, _ipRegex) || IsValidHost(hostNameOrAddress, _urlRegex);
    }

    private static bool IsValidHost(string url, string pattern)
    {
        var reg = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        return reg.IsMatch(url);
    }

    public async Task StopPingOnNavigateAwayAsync()
    {
        if (IsPingStarted)
        {
            await StartPingAsync(); // Toggle off
        }
    }

    #endregion
}

