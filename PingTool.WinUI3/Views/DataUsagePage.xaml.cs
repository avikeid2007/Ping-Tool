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
        await LoadDataUsageAsync();
    }

    private async Task LoadDataUsageAsync()
    {
        try
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();
            if (icp == null) return;

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
                TodayDownload.Text = $"{todayUsage.BytesReceived / 1048576} MB";
                TodayUpload.Text = $"{todayUsage.BytesSent / 1048576} MB";
            }

            // Last 10 days usage
            var totalStart = DateTime.Now.AddDays(-10).Date;
            var totalEnd = DateTime.Now.Date;
            var totalUsage = (await icp.GetNetworkUsageAsync(totalStart, totalEnd, DataUsageGranularity.Total, networkUsageStates)).FirstOrDefault();
            
            if (totalUsage != null)
            {
                TotalDownload.Text = $"{totalUsage.BytesReceived / 1048576} MB";
                TotalUpload.Text = $"{totalUsage.BytesSent / 1048576} MB";
            }
        }
        catch
        {
            // Ignore errors
        }
    }
}
