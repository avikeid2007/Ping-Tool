using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace PingBackground
{
    class Program
    {
        private static AppServiceConnection connection;
        public static bool IsPingStop { get; set; }
        public static string HostNameOrAddress { get; private set; }

        static async Task Main(string[] args)
        {
            await InitializeAppServiceConnectionAsync();
            await ThreadProcAsync();
        }

        static async Task ThreadProcAsync()
        {
            var pingSender = new Ping();
            while (!IsPingStop)
            {
                try
                {
                    if (!string.IsNullOrEmpty(HostNameOrAddress))
                    {
                        var reply = await pingSender.SendPingAsync(HostNameOrAddress);
                        ValueSet request = new ValueSet();
                        request.Add("host", reply.Address.ToString());
                        request.Add("time", reply.RoundtripTime);
                        request.Add("size", reply.Buffer.Length);
                        request.Add("ttl", reply.Options?.Ttl);
                        request.Add("response", $"Reply from {reply.Address}: bytes={ reply.Buffer.Length} time={ reply.RoundtripTime}ms TTL={reply.Options?.Ttl}");
                        await connection.SendMessageAsync(request);
                    }
                }
                catch
                {
                    var reply = await pingSender.SendPingAsync(HostNameOrAddress);
                    ValueSet request = new ValueSet();
                    request.Add("host", reply.Address.ToString());
                    request.Add("time", -1);
                    request.Add("size", 0);
                    request.Add("ttl", 0);
                    request.Add("response", "Request timed out.");
                    await connection.SendMessageAsync(request);
                }
                finally
                {
                    await Task.Delay(800);
                }
            }
        }
        private static async Task InitializeAppServiceConnectionAsync()
        {
            connection = new AppServiceConnection
            {
                AppServiceName = "PingInteropService",
                PackageFamilyName = Package.Current.Id.FamilyName
            };
            connection.RequestReceived += Connection_RequestReceived;
            connection.ServiceClosed += Connection_ServiceClosed;
            await connection.OpenAsync();
        }

        private static void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            Environment.Exit(0);
        }
        private static void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            HostNameOrAddress = Convert.ToString(args.Request.Message["host"]);
            IsPingStop = Convert.ToBoolean(args.Request.Message["isStop"]);
        }
    }
}
