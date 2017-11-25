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
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

using SkynetTCP.Models;
using SkynetTCP.Services.Interfaces;
using SkynetTCP.API;

namespace SkynetTCP.Core
{
    public class SkyNetTcpServer
    {
        private readonly ISerializerService _serializer;
        private List<Task> taskList = new List<Task>();
        private TcpListener tcpListener = new TcpListener(new IPEndPoint(IPAddress.Any, 25000));
        public ConcurrentDictionary<string, NetworkStream> roomDic = new ConcurrentDictionary<string, NetworkStream>();
        private SkynetClient _skynetClient;
        private CloudQueue _queue;

        public SkyNetTcpServer(
            ISerializerService serializer, 
            string queueName,
            SkynetClient skynetClient)
        {
            _serializer = serializer;

            _skynetClient = skynetClient;

            var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=skynetgdl;AccountKey=KVJGcGdkiUg6rhDyDbvbgb5YfCf3zaQX3z78K5YFrW4zmjaGzAnUlZwCna4k7nhuq9sZU6uqb7dHdi3S5EODvw==;EndpointSuffix=core.windows.net");
            var cloudClient = storageAccount.CreateCloudQueueClient();
            _queue = cloudClient.GetQueueReference(queueName);
        }

        public bool IsListening => tcpListener.Server.IsBound;

        public EndPoint EndPoint => tcpListener.Server.LocalEndPoint;

        public async Task StartQueue()
        {
            while (true)
            {
                var message = await _queue.GetMessageAsync();
                if (message != null)
                {
                    var json = message.AsString;
                    var parsed = JsonConvert.DeserializeObject<ActionMessage>(json);
                    if (parsed.Action == ACTIONS.TELL || parsed.Action == ACTIONS.CONFIGURE)
                    {
                        await _queue.DeleteMessageAsync(message);
                        await SendToClient(parsed);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        public async Task Start()
        {
            tcpListener.Start();

            taskList.Add(StartQueue());

            while (true)
            {
                var tcpClient = await tcpListener.AcceptTcpClientAsync();
                Console.WriteLine("Connected");
                taskList.Add(ReceiveClient(tcpClient));
            }
        }

        private async Task ReceiveClient(TcpClient tcpClient)
        {
            var buffer = new byte[256];

            var stream = tcpClient.GetStream();

            int i;
            while ((i = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                var obj = _serializer.Deserialize<ActionMessage>(buffer);

                buffer = new byte[256];

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
                case ACTIONS.ACKNOWLEDGE:
                    await _skynetClient.PushUpdate(actionMessage.Name, actionMessage.Do);
                    break;
                default:
                    break;
            }
        }

        private async Task SendConfig(ActionMessage actionMessage, NetworkStream stream)
        {
            var config = await _skynetClient.GetConfig(actionMessage.Name);

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
