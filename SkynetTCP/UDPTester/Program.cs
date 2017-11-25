using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;

using SkynetTCP.Models;
using SkynetTCP.Services;

namespace UDPTester
{
    class Program
    {
        static UdpClient _client = new UdpClient()
        {
            EnableBroadcast = true
        };

        static void Main(string[] args)
        {
            var message = new ActionMessage
            {
                Do = "",
                Action = ACTIONS.HELLO,
                Name = ""
            };

            var serializer = new SerializerService();
            var bytes = serializer.Serialize(message);

            var task = _client.SendAsync(
                bytes, 
                bytes.Length, 
                new IPEndPoint(IPAddress.Parse("192.168.1.255"), 25500))
                .GetAwaiter();

            task.OnCompleted(() =>
            {
                Console.WriteLine("Sent first message");

                var response = _client
                .ReceiveAsync()
                .GetAwaiter();

                response.OnCompleted(() =>
                {
                    var result = response.GetResult();
                    var deserialized = serializer.Deserialize<ActionMessage>(result.Buffer);

                    Console.WriteLine(JsonConvert.SerializeObject(deserialized));
                });

                while (!response.IsCompleted)
                {
                    Console.WriteLine("Waiting for response");
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }

            });
            
            Console.Read();
        }
    }
}
