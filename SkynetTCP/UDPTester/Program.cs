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
        static UdpClient _client = new UdpClient(new IPEndPoint(IPAddress.Any, 0))
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

            Console.WriteLine(IPAddress.Broadcast);

            var task = _client.SendAsync(bytes, bytes.Length, new IPEndPoint(IPAddress.Broadcast, 25500)).GetAwaiter();
            task.OnCompleted(() =>
            {
                Console.WriteLine("Sent first message");
                var response = _client.ReceiveAsync().GetAwaiter();
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
