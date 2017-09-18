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

namespace SkynetTCP.Core
{
    public class SkyNetTcpServer
    {
        private readonly ISerializerService _serializer;
        List<Task> taskList = new List<Task>();
        TcpListener tcpListener = new TcpListener(new IPEndPoint(IPAddress.Any, 25000));
        ConcurrentDictionary<string, NetworkStream> roomDic = new ConcurrentDictionary<string, NetworkStream>();
        IQueueClient queue;

        public SkyNetTcpServer(ISerializerService serializer, string queueName)
        {
            _serializer = serializer;

            queue = new QueueClient("Endpoint=sb://skynet.servicebus.windows.net/;SharedAccessKeyName=Rasp;SharedAccessKey=sn5Oiv67fiKXRz1iM5Zzf0jz24134PF+qoR9mmM4NGQ=;", "Queue1");
            queue.RegisterMessageHandler(ReceiveMessageFromQueue, new MessageHandlerOptions(HandleError)
            {
                AutoComplete = false
            });
        }

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

                await ProcessMessage(obj, stream);
            }
        }

        private async Task ProcessMessage(ActionMessage actionMessage, NetworkStream stream = null)
        {
            switch (actionMessage?.Action)
            {
                case ACTIONS.CONFIGURE:
                    await SendConfig(actionMessage);
                    break;
                case ACTIONS.CONNECT:
                    {
                        if(stream != null)
                        {
                            roomDic.TryAdd(actionMessage.Name, stream);
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

        private Task GetConfig()
        {
            throw new NotImplementedException();
        }

        private Task SendConfig(ActionMessage actionMessage)
        {
            throw new NotImplementedException();
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
