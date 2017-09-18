using System;
using System.Threading;
using System.Threading.Tasks;

using SkynetTCP.Core;
using SkynetTCP.Services;

namespace SkynetTCP
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new SkyNetTcpServer(new SerializerService(), "Queue1");
            var udpServer = new SkyNetUdpServer(new SerializerService());

            var tcpTask = server.Start();
            var udpTask = udpServer.Start();

            var statusTask = Task.Run(() =>
            {
                while (true)
                {
                    Console.WriteLine("Running");
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            });

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
