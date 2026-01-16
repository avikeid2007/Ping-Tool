using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Networking.Connectivity;

namespace PingTool.Views;

public sealed partial class DataUsagePage : Page
{
    public DataUsagePage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await LoadDataAsync();
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();
            if (icp == null)
            {
                NetworkName.Text = "No Connection";
                return;
            }

            // Connection Info
            NetworkName.Text = icp.ProfileName ?? "Unknown";
            
            var connectionCost = icp.GetConnectionCost();
            CostType.Text = connectionCost.NetworkCostType switch
            {
                NetworkCostType.Unrestricted => "Unlimited",
                NetworkCostType.Fixed => "Fixed",
                NetworkCostType.Variable => "Metered",
                _ => "Unknown"
            };

            // Connection Type
            if (icp.IsWlanConnectionProfile)
            {
                ConnectionType.Text = "Wi-Fi";
                var signal = icp.GetSignalBars();
                if (signal.HasValue)
                {
                    SignalStrength.Text = $"{signal.Value * 20}%";
                    SignalIcon.Glyph = signal.Value >= 3 ? "\uE871" : (signal.Value >= 2 ? "\uEC3F" : "\uEC3E");
                }
            }
            else if (icp.IsWwanConnectionProfile)
            {
                ConnectionType.Text = "Cellular";
                var signal = icp.GetSignalBars();
                if (signal.HasValue)
                {
                    SignalStrength.Text = $"{signal.Value * 20}%";
                }
            }
            else
            {
                ConnectionType.Text = "Ethernet";
                SignalStrength.Text = "100%";
            }

            var networkUsageStates = new NetworkUsageStates
            {
                Roaming = TriStates.DoNotCare,
                Shared = TriStates.DoNotCare
            };

            // Today's usage
            var todayStart = DateTime.Now.Date;
            var todayEnd = DateTime.Now.Date.AddDays(1);
            var todayUsage = (await icp.GetNetworkUsageAsync(todayStart, todayEnd, DataUsageGranularity.Total, networkUsageStates)).FirstOrDefault();
            
            if (todayUsage != null)
            {
                var todayDown = todayUsage.BytesReceived / 1048576.0;
                var todayUp = todayUsage.BytesSent / 1048576.0;
                TodayDownload.Text = FormatBytes(todayUsage.BytesReceived);
                TodayUpload.Text = FormatBytes(todayUsage.BytesSent);
                TodayTotal.Text = FormatBytes(todayUsage.BytesReceived + todayUsage.BytesSent);
            }

            // Last 30 days usage
            var monthStart = DateTime.Now.AddDays(-30).Date;
            var monthEnd = DateTime.Now.Date.AddDays(1);
            var monthUsage = (await icp.GetNetworkUsageAsync(monthStart, monthEnd, DataUsageGranularity.Total, networkUsageStates)).FirstOrDefault();
            
            if (monthUsage != null)
            {
                MonthDownload.Text = FormatBytes(monthUsage.BytesReceived);
                MonthUpload.Text = FormatBytes(monthUsage.BytesSent);
                MonthTotal.Text = FormatBytes(monthUsage.BytesReceived + monthUsage.BytesSent);
                
                // Daily average
                var totalBytes = monthUsage.BytesReceived + monthUsage.BytesSent;
                var dailyAvg = totalBytes / 30.0;
                DailyAverage.Text = $"{FormatBytes((ulong)dailyAvg)}/day";
            }

            // Get daily breakdown for peak day
            var dailyUsage = await icp.GetNetworkUsageAsync(monthStart, monthEnd, DataUsageGranularity.PerDay, networkUsageStates);
            if (dailyUsage.Any())
            {
                var peakUsage = dailyUsage.OrderByDescending(u => u.BytesReceived + u.BytesSent).First();
                var peakIndex = dailyUsage.ToList().IndexOf(peakUsage);
                var peakDate = monthStart.AddDays(peakIndex);
                PeakDay.Text = peakDate.ToString("MMM dd");
            }

            LastUpdated.Text = DateTime.Now.ToString("h:mm tt");
        }
        catch
        {
            NetworkName.Text = "Error loading data";
        }
    }

    private static string FormatBytes(ulong bytes)
    {
        if (bytes >= 1073741824) // GB
            return $"{bytes / 1073741824.0:F1} GB";
        else if (bytes >= 1048576) // MB
            return $"{bytes / 1048576.0:F0} MB";
        else if (bytes >= 1024) // KB
            return $"{bytes / 1024.0:F0} KB";
        else
            return $"{bytes} B";
    }
}
