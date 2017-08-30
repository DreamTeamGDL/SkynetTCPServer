using System;
using System.Threading;
using System.Threading.Tasks;

using TCPServer.Core;

namespace TCPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new SkyNetTcpServer("Queue1");

            var task = server.Start();

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
