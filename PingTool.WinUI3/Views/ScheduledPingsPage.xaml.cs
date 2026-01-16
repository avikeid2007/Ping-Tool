using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using PingTool.Models;
using PingTool.Services;
using System.Collections.ObjectModel;

namespace PingTool.Views;

public sealed partial class ScheduledPingsPage : Page
{
    private static ScheduledPingService? _sharedService;
    private readonly ScheduledPingService _service;

    public ObservableCollection<ScheduledPing> Schedules { get; } = new();

    public ScheduledPingsPage()
    {
        InitializeComponent();
        
        // Use shared service instance for persistence across navigation
        _sharedService ??= new ScheduledPingService();
        _service = _sharedService;

        _service.PingCompleted += OnPingCompleted;
        _service.PingFailed += OnPingFailed;

        LoadSchedules();
    }

    private void LoadSchedules()
    {
        Schedules.Clear();
        foreach (var ping in _service.ScheduledPings)
        {
            Schedules.Add(ping);
        }
        UpdateEmptyState();
    }

    private void UpdateEmptyState()
    {
        EmptyState.Visibility = Schedules.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnPingCompleted(ScheduledPing ping, bool success, long latency)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            // Refresh the list to show updated status
            var index = Schedules.ToList().FindIndex(p => p.Id == ping.Id);
            if (index >= 0)
            {
                Schedules[index] = ping;
            }
        });
    }

    private void OnPingFailed(ScheduledPing ping)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            // Show notification for failed ping
            try
            {
                var toastXml = new Windows.Data.Xml.Dom.XmlDocument();
                toastXml.LoadXml($@"
                    <toast>
                        <visual>
                            <binding template='ToastGeneric'>
                                <text>Scheduled Ping Failed</text>
                                <text>Unable to reach {ping.Host} - {ping.ConsecutiveFailures} consecutive failures</text>
                            </binding>
                        </visual>
                    </toast>");

                var toast = new Windows.UI.Notifications.ToastNotification(toastXml);
                Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier("Ping Legacy").Show(toast);
            }
            catch
            {
                // Ignore notification errors
            }
        });
    }

    private void AddSchedule_Click(object sender, RoutedEventArgs e)
    {
        AddNewSchedule();
    }

    private void NewHostInput_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            AddNewSchedule();
        }
    }

    private void AddNewSchedule()
    {
        var host = NewHostInput.Text?.Trim();
        if (string.IsNullOrEmpty(host)) return;

        var interval = 5;
        if (IntervalComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tagStr)
        {
            int.TryParse(tagStr, out interval);
        }

        var ping = new ScheduledPing
        {
            Host = host,
            IntervalMinutes = interval,
            IsEnabled = true,
            NotifyOnFailure = true
        };

        _service.AddScheduledPing(ping);
        Schedules.Add(ping);
        NewHostInput.Text = string.Empty;
        UpdateEmptyState();
    }

    private async void RunNow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string id)
        {
            await _service.RunNowAsync(id);
            LoadSchedules(); // Refresh to show updated status
        }
    }

    private void Toggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggle && toggle.Tag is string id)
        {
            _service.ToggleEnabled(id);
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string id)
        {
            _service.RemoveScheduledPing(id);
            var item = Schedules.FirstOrDefault(p => p.Id == id);
            if (item != null)
            {
                Schedules.Remove(item);
            }
            UpdateEmptyState();
        }
    }
}
