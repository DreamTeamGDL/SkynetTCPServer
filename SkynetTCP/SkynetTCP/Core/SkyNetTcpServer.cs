﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

using TCPServer.Models;
using System.Threading;

namespace TCPServer.Core
{
    public class SkyNetTcpServer
    {
        List<Task> taskList = new List<Task>();
        TcpListener tcpListener = new TcpListener(new IPEndPoint(IPAddress.Any, 25000));
        ConcurrentDictionary<string, NetworkStream> roomDic = new ConcurrentDictionary<string, NetworkStream>();
        IQueueClient queue;

        public SkyNetTcpServer(string queueName)
        {
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
            var json = Encoding.UTF8.GetString(message.Body);

            Console.WriteLine(json);

            var obj = JsonConvert.DeserializeObject<ActionMessage>(json);

            await ProcessMessage(obj);


            await queue.CompleteAsync(message.SystemProperties.LockToken);
        }

        private async Task ReceiveClient(TcpClient tcpClient)
        {
            var bytes = new byte[256];
            string data = null;

            var stream = tcpClient.GetStream();

            int i;
            while ((i = await stream.ReadAsync(bytes, 0, bytes.Length)) != 0)
            {
                data = Encoding.ASCII.GetString(bytes, 0, i);

                Console.WriteLine(data);

                var obj = JsonConvert.DeserializeObject<Models.ActionMessage>(data);

                await ProcessMessage(obj, stream);
            }
        }

        private async Task ProcessMessage(ActionMessage actionMessage, NetworkStream stream = null)
        {
            switch (actionMessage?.Action)
            {
                case "Connect":
                    {
                        if(stream != null)
                        {
                            roomDic.TryAdd(actionMessage.Name, stream);
                        }
                    }
                    break;
                case "Tell":
                    await SendToClient(actionMessage.Name, actionMessage.Do);
                    break;
                default:
                    break;
            }
        }

        private async Task SendToClient(string clientName, string thingTodo)
        {
            if (roomDic.ContainsKey(clientName))
            {
                var buffer = Encoding.ASCII.GetBytes(thingTodo);
                await roomDic[clientName].WriteAsync(buffer, 0, buffer.Length);
            }
        }
    }
}