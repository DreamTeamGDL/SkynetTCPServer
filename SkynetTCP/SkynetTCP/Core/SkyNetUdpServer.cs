using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Linq;

using SkynetTCP.Models;
using SkynetTCP.Services.Interfaces;

namespace SkynetTCP.Core
{
    public class SkyNetUdpServer
    {
        private readonly int TCP_PORT = 25000;
        private UdpReceiveResult receivedByteData;
        private readonly ISerializerService _serializer;
        private string myIP = Dns.GetHostAddresses(Dns.GetHostName()).Where(add => add.AddressFamily != AddressFamily.InterNetworkV6).First().ToString();
        private readonly UdpClient client = new UdpClient(new IPEndPoint(IPAddress.Any, 25500))
        {
            EnableBroadcast = true
        };

        public SkyNetUdpServer(ISerializerService serializer)
        {
            _serializer = serializer;   
        }

        public async Task Start()
        {
            try
            {
                Console.WriteLine("Receiving UDP connection");
                while (true)
                {
                    receivedByteData = await client.ReceiveAsync();
                    var message = _serializer.Deserialize<ActionMessage>(receivedByteData.Buffer);
                    if (message.Action == ACTIONS.HELLO)
                    {
                        var connectAction = new ActionMessage
                        {
                            Action = ACTIONS.CONNECT,
                            Name = myIP,
                            Do = TCP_PORT.ToString()
                        };
                        
                        var datagram = _serializer.Serialize(connectAction);

                        client.Connect(receivedByteData.RemoteEndPoint);
                        await client.SendAsync(datagram, datagram.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
