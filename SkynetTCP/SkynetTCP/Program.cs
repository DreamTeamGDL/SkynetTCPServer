using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

using SkynetTCP.API;
using SkynetTCP.Core;
using SkynetTCP.Services;

namespace SkynetTCP
{
    class Program
    {
        static void Main(string[] args)
        {
            var skynetClient = new SkynetClient();
            skynetClient.GetZoneID(GetMac()).GetAwaiter().GetResult();

            var server = new SkyNetTcpServer(new SerializerService(), "mainqueue", skynetClient);

            var tcpTask = server.Start();
            var queueTask = server.StartQueue();

            var statusTask = Task.Run(() =>
            {
                while (true)
                {
                    if (server.IsListening) Console.WriteLine($"TCP: Running and connected on {server.EndPoint.ToString()}");
                    foreach (var client in server.roomDic.Keys)
                    {
                        Console.WriteLine($"{client}");
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            });

            Thread.Sleep(Timeout.Infinite);
        }

        private static string GetMac()
            => NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(nic =>
                (nic.OperationalStatus == OperationalStatus.Up)
                &&
                (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
            .Last()
            .GetPhysicalAddress()
            .ToString();
    }
}
