using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using System.Threading;

using SkynetTCP.Models;
using SkynetTCP.Services.Interfaces;
using SkynetTCP.API;

namespace SkynetTCP.Core
{
    public class SkyNetTcpServer
    {
        private readonly ISerializerService _serializer;
        List<Task> taskList = new List<Task>();
        TcpListener tcpListener = new TcpListener(new IPEndPoint(IPAddress.Any, 25000));
        ConcurrentDictionary<string, NetworkStream> roomDic = new ConcurrentDictionary<string, NetworkStream>();
        IQueueClient queue;
        SkynetClient skynet;

        public SkyNetTcpServer(
            ISerializerService serializer, 
            string queueName,
            string zoneID)
        {
            _serializer = serializer;

            queue = new QueueClient("Endpoint=sb://skynet.servicebus.windows.net/;SharedAccessKeyName=Rasp;SharedAccessKey=sn5Oiv67fiKXRz1iM5Zzf0jz24134PF+qoR9mmM4NGQ=;", "Queue1");
            queue.RegisterMessageHandler(ReceiveMessageFromQueue, new MessageHandlerOptions(HandleError)
            {
                AutoComplete = false
            });

            skynet = new SkynetClient(zoneID);
        }

        public bool IsListening => tcpListener.Server.IsBound;

        public EndPoint EndPoint => tcpListener.Server.LocalEndPoint;

        public async Task Start()
        {
            tcpListener.Start();

            while (true)
            {
                var tcpClient = await tcpListener.AcceptTcpClientAsync();
                taskList.Add(ReceiveClient(tcpClient));
            }
        }

        private Task HandleError(ExceptionReceivedEventArgs args)
        {
            return Task.CompletedTask;
        }

        private async Task ReceiveMessageFromQueue(Message message, CancellationToken token)
        {
            var obj = _serializer.Deserialize<ActionMessage>(message.Body);

            await ProcessMessage(obj);
            
            await queue.CompleteAsync(message.SystemProperties.LockToken);
        }

        private async Task ReceiveClient(TcpClient tcpClient)
        {
            var bytes = new byte[256];

            var stream = tcpClient.GetStream();

            int i;
            while ((i = await stream.ReadAsync(bytes, 0, bytes.Length)) != 0)
            {
                var obj = _serializer.Deserialize<ActionMessage>(bytes);

                bytes = new byte[256];

                await ProcessMessage(obj, stream);
            }
        }

        private async Task ProcessMessage(ActionMessage actionMessage, NetworkStream stream = null)
        {
            switch (actionMessage?.Action)
            {
                case ACTIONS.CONFIGURE:
                    await SendConfig(actionMessage, stream);
                    break;
                case ACTIONS.CONNECT:
                    {
                        if(stream != null)
                        {
                            if(roomDic.TryAdd(actionMessage.Name, stream))
                            {
                                await SendToClient(new ActionMessage
                                {
                                    Action = ACTIONS.TELL,
                                    Do = "",
                                    Name = actionMessage.Name
                                });
                            }
                        }
                    }
                    break;
                case ACTIONS.TELL:
                    await SendToClient(actionMessage);
                    break;
                default:
                    break;
            }
        }

        public Task SendToChris()
        {
            return SendToClient(new ActionMessage
            {
                Action = ACTIONS.TELL,
                Do = "LIGHT YAGAMI;TRUE",
                Name = "Chris Rasp"
            });
        }

        private async Task SendConfig(ActionMessage actionMessage, NetworkStream stream)
        {
            var config = await skynet.GetConfig(actionMessage.Name);

            var buffer = _serializer.Serialize(config);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        private async Task SendToClient(ActionMessage message)
        {
            if (roomDic.ContainsKey(message.Name))
            {
                var buffer = _serializer.Serialize(message);
                await roomDic[message.Name].WriteAsync(buffer, 0, buffer.Length);
            }
        }
    }
}
