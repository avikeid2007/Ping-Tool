using BasicMvvm;
using BasicMvvm.Commands;
using PingTool.Helpers;
using PingTool.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Networking.Connectivity;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace PingTool
{
    public class MainViewModel : BindableBase
    {
        public ConnectionProfile connectionProfile { get; set; }
        private string _ipAddress;
        private string _profileName;
        private bool _isWlan;
        private long _totalReceivedBytes;
        private long _totalSentBytes;
        private bool _isSupportIPV6;
        private int _wifiBars;
        private string _hostNameOrAddress;
        private string _ipType;
        public MainViewModel()
        {
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
        }
        public string IpType
        {
            get { return _ipType; }
            set
            {
                _ipType = value;
                OnPropertyChanged();
            }
        }
        public string HostNameOrAddress
        {
            get { return _hostNameOrAddress; }
            set
            {
                _hostNameOrAddress = value;
                OnPropertyChanged();
            }
        }
        public long TotalReceivedBytes
        {
            get { return _totalReceivedBytes; }
            set
            {
                _totalReceivedBytes = value;
                OnPropertyChanged();
            }
        }
        public long TotalSentBytes
        {
            get { return _totalSentBytes; }
            set
            {
                _totalSentBytes = value;
                OnPropertyChanged();
            }
        }
        public bool IsSupportIPV6
        {
            get { return _isSupportIPV6; }
            set
            {
                _isSupportIPV6 = value;
                OnPropertyChanged();
            }
        }
        public int WifiBars
        {
            get { return _wifiBars; }
            set
            {
                _wifiBars = value;
                OnPropertyChanged();
            }
        }
        public bool IsWlan
        {
            get { return _isWlan; }
            set
            {
                _isWlan = value;
                OnPropertyChanged();
            }
        }

        public string IpAddress
        {
            get { return _ipAddress; }
            set
            {
                _ipAddress = value;
                OnPropertyChanged();
            }
        }
        public string ProfileName
        {
            get { return _profileName; }
            set
            {
                _profileName = value;
                OnPropertyChanged();
            }
        }
        private bool _isPingStarted;
        public bool IsPingStarted
        {
            get { return _isPingStarted; }
            set
            {
                _isPingStarted = value;
                OnPropertyChanged();
            }
        }
        public ICommand StartCommand => new AsyncCommand(OnStartCommandExecutedAsync);
        public ICommand ClearCommand => new DelegateCommand(OnClearCommandExecuted);
        public ICommand CopyCommand => new DelegateCommand(OnCopyCommandExecuted);
        public ICommand ExportCommand => new AsyncCommand(OnExportCommandExecuteAsync);
        public ICommand KeyDownCommand => new AsyncCommand<KeyRoutedEventArgs>(OnKeyDownCommandExecuteAsync);
        public async Task OnNavigatedToAsync(NavigationEventArgs _)
        {
            HostNameOrAddress = "172.217.166.238";
            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
            {
                App.AppServiceConnected += MainPage_AppServiceConnected;
                App.AppServiceDisconnected += MainPage_AppServiceDisconnected;
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            }
            SetNetworkInfo();
        }

        private void SetNetworkInfo()
        {
            try
            {
                var icp = NetworkInformation.GetInternetConnectionProfile();
                if (icp?.NetworkAdapter == null) return;
                var hostname =
                    NetworkInformation.GetHostNames()
                        .FirstOrDefault(
                            hn =>
                                hn.IPInformation?.NetworkAdapter != null && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                                == icp.NetworkAdapter.NetworkAdapterId);
                var LocalLANColl = NetworkInterface.GetAllNetworkInterfaces();
                var LocalLAN = LocalLANColl.FirstOrDefault(x => x.Id.ToUpper() == "{" + hostname.IPInformation.NetworkAdapter.NetworkAdapterId.ToString().ToUpper() + "}");
                IpAddress = hostname?.CanonicalName;
                IpType = hostname?.Type.ToString();
                ProfileName = icp.ProfileName;
                IsWlan = icp.IsWlanConnectionProfile;
                WifiBars = Convert.ToInt32(icp.GetSignalBars());
                var ipstats = LocalLAN.GetIPStatistics();
                TotalReceivedBytes = GetSizeInByte(ipstats.BytesReceived);
                TotalSentBytes = GetSizeInByte(ipstats.BytesSent);
                IsSupportIPV6 = LocalLAN.Supports(NetworkInterfaceComponent.IPv6);
            }
            catch (Exception ex)
            {
            }
        }

        private void NetworkInformation_NetworkStatusChanged(object sender)
        {
            SetNetworkInfo();
        }
        //private async Task<IEnumerable<DeviceDefinition>> GetConnectedDeviceDefinitionsAsync()
        //{
        //    var aqsFilter = "System.Devices.Aep.ProtocolId:=\"{37aba761-2124-454c-8d82-c42962c2de2b}\"";

        //    var deviceInformationCollection = await DeviceInformation.FindAllAsync(aqsFilter);
        //    var deviceIds = deviceInformationCollection.Select(d => new DeviceDefinition { DeviceId = d.Id, DeviceType = d.Name }).ToList();
        //    return deviceIds;
        //}
        private long GetSizeInByte(long bytes)
        {
            return bytes / 1048576;
        }
        private void MainPage_AppServiceConnected(object sender, AppServiceTriggerDetails e)
        {
            App.Connection.RequestReceived += AppServiceConnection_RequestReceivedAsync;
        }
        private void MainPage_AppServiceDisconnected(object sender, EventArgs e)
        {
            //sdf
        }
        private async void AppServiceConnection_RequestReceivedAsync(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var reply = new PingMassage
            {
                IpAddress = Convert.ToString(args.Request.Message["host"]),
                Time = Convert.ToInt64(args.Request.Message["time"]),
                Size = Convert.ToInt32(args.Request.Message["size"]),
                Ttl = Convert.ToInt32(args.Request.Message["ttl"]),
                Response = Convert.ToString(args.Request.Message["response"]),
            };
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                                () => { (PingCollaction ?? (PingCollaction = new ObservableCollection<PingMassage>())).Add(reply); });
        }
        private async Task OnStartCommandExecutedAsync()
        {
            if (!string.IsNullOrEmpty(HostNameOrAddress))
            {
                ValueSet request = new ValueSet();
                if (!IsPingStarted)
                {
                    request.Add("host", HostNameOrAddress);
                    request.Add("isStop", "false");
                    await App.Connection.SendMessageAsync(request);
                    IsPingStarted = true;
                }
                else
                {
                    request.Add("host", "");
                    request.Add("isStop", "false");
                    await App.Connection.SendMessageAsync(request);
                    IsPingStarted = false;
                }
            }
        }
        private void OnClearCommandExecuted()
        {
            PingCollaction?.Clear();
        }
        private async Task OnKeyDownCommandExecuteAsync(KeyRoutedEventArgs obj)
        {
            if (obj.Key == VirtualKey.Enter)
            {
                await OnStartCommandExecutedAsync();
            }
        }
        private async Task OnExportCommandExecuteAsync()
        {
            if (PingCollaction?.Count > 0)
            {
                var text = string.Empty;
                PingCollaction.ToList().ForEach(x => text += x.Response + "\r");
                await FileHelper.SaveFileAsync(text, "ping.txt");
            }
        }
        private void OnCopyCommandExecuted()
        {
            FileHelper.CopyText(IpAddress ?? "");
        }
        private ObservableCollection<PingMassage> _pingCollaction;
        public ObservableCollection<PingMassage> PingCollaction
        {
            get { return _pingCollaction; }
            set
            {
                _pingCollaction = value;
                OnPropertyChanged();
            }
        }
        public bool IsPingStop { get; set; }
        public async Task StartPingAsync()
        {
            ValueSet request = new ValueSet();
            request.Add("host", "8.8.8.8");
            request.Add("isStop", "false");
            await App.Connection.SendMessageAsync(request);
        }
    }
}
