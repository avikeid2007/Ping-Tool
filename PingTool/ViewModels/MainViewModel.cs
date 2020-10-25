using BasicMvvm;
using BasicMvvm.Commands;
using PingTool.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.AppService;
using Windows.Devices.Enumeration;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.UI.Core;
using Windows.UI.Xaml.Navigation;

namespace PingTool
{
    public class MainViewModel : BindableBase
    {
        public ConnectionProfile connectionProfile { get; set; }
        private string _ipAddress;
        private string _profileName;
        private bool _isWlan;


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
        public MainViewModel()
        {

        }
        string HostNameOrAddress = string.Empty;
        public ICommand StartCommand => new AsyncCommand(OnStartCommandExecutedAsync);
        public ICommand StopCommand => new AsyncCommand(OnStopCommandExecutedAsync);
        public async Task OnNavigatedToAsync(NavigationEventArgs e)
        {
            connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            var ss = NetworkInformation.GetHostNames().ToList();
            var ds = NetworkInformation.GetLanIdentifiers().ToList();
            SetNetworkInfo();
            var sdfd = connectionProfile.GetDomainConnectivityLevel();
            var sdfr = connectionProfile.GetNetworkNames().ToList();
            var dsfds = connectionProfile.GetConnectionCost();
            var sdfeer = connectionProfile.GetDataPlanStatus();
            var dataPlanStatus = connectionProfile.GetDataPlanStatus();
            ulong? outboundBandwidth = dataPlanStatus.OutboundBitsPerSecond;
            if (outboundBandwidth.HasValue)
            {
                System.Diagnostics.Debug.WriteLine("OutboundBitsPerSecond : " + outboundBandwidth + "\n");
                System.Diagnostics.Debug.WriteLine("OutboundBitsPerSecond : " + outboundBandwidth + "\n");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("OutboundBitsPerSecond : Not Defined\n");
            }
            var sssd = NetworkInterface.GetAllNetworkInterfaces();
            //if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
            //{
            //    App.AppServiceConnected += MainPage_AppServiceConnected;
            //    App.AppServiceDisconnected += MainPage_AppServiceDisconnected;
            //    await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            //}
        }
        private async Task enumerateSnapshot()
        {
            DeviceInformationCollection collection = await DeviceInformation.FindAllAsync();
        }
        private async void SetNetworkInfo()
        {
            try
            {
                await enumerateSnapshot();

                var icp = NetworkInformation.GetInternetConnectionProfile();
                if (icp?.NetworkAdapter == null) return;
                var hostname =
                    NetworkInformation.GetHostNames()
                        .FirstOrDefault(
                            hn =>
                                hn.IPInformation?.NetworkAdapter != null && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                                == icp.NetworkAdapter.NetworkAdapterId);

                var sssd = NetworkInterface.GetAllNetworkInterfaces();
                IpAddress = hostname?.CanonicalName;
                ProfileName = icp.ProfileName;
                IsWlan = icp.IsWlanConnectionProfile;
                var NetworkId = hostname.IPInformation.NetworkAdapter;
                var df = icp.NetworkAdapter;
                var dfg = icp.GetSignalBars();
                var dfds = icp.NetworkSecuritySettings;
            }
            catch (Exception ex)
            {
            }
        }
        private void MainPage_AppServiceConnected(object sender, AppServiceTriggerDetails e)
        {
            App.Connection.RequestReceived += AppServiceConnection_RequestReceived;
        }

        private void MainPage_AppServiceDisconnected(object sender, EventArgs e)
        {
            //sdf
        }
        private async void AppServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
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
            ValueSet request = new ValueSet();
            request.Add("host", "8.8.8.8");
            request.Add("isStop", "false");
            await App.Connection.SendMessageAsync(request);
        }
        private async Task OnStopCommandExecutedAsync()
        {
            ValueSet request = new ValueSet();
            request.Add("host", "");
            request.Add("isStop", "false");
            await App.Connection.SendMessageAsync(request);
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
