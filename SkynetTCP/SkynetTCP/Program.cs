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
            var server = new SkyNetTcpServer(new SerializerService(), "Queue1", Guid.NewGuid().ToString());
            var udpServer = new SkyNetUdpServer(new SerializerService());

            var tcpTask = server.Start();
            var udpTask = udpServer.Start();

            var statusTask = Task.Run(() =>
            {
                while (true)
                {
                    if (server.IsListening) Console.WriteLine($"TCP: Running and connected on {server.EndPoint.ToString()}");
                    if (udpServer.IsListening) Console.WriteLine($"UDP: Running and connected on {udpServer.UdpEndpoint.ToString()}");
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            });

            /*
            Console.ReadLine();
            var awaiter = server.SendToChris().GetAwaiter();
            awaiter.OnCompleted(() =>
            {
                Console.WriteLine("Sent");
            });
            */

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
