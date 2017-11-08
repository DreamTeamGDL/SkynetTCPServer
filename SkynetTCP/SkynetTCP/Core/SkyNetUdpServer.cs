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
        private string myIP;
        private UdpClient client = new UdpClient(new IPEndPoint(IPAddress.Any, 25500))
        {
            EnableBroadcast = true
        };

        public SkyNetUdpServer(ISerializerService serializer)
        {
            _serializer = serializer;
            myIP = Dns.GetHostName();
        }

        public bool IsListening => client.Client.IsBound;

        public EndPoint UdpEndpoint => client.Client.LocalEndPoint;

        public async Task Start()
        {
            try
            {
                while (true)
                {
                    receivedByteData = await client.ReceiveAsync();
                    Console.WriteLine(receivedByteData.RemoteEndPoint);
                    var message = _serializer.Deserialize<ActionMessage>(receivedByteData.Buffer);
                    if (message.Action == ACTIONS.HELLO)
                    {
                        var connectAction = new ActionMessage
                        {
                            Action = ACTIONS.CONNECT,
                            Name = myIP,
                            Do = TCP_PORT.ToString()
                        };
                        
                        using (var tempClient = new UdpClient())
                        {
                            var datagram = _serializer.Serialize(connectAction);
                            tempClient.Connect(receivedByteData.RemoteEndPoint);
                            await tempClient.SendAsync(datagram, datagram.Length);
                            tempClient.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}
