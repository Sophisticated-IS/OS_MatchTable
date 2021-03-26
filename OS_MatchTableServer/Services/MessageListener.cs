using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MessageHandler;
using Messages.Base;
using Messages.ClientMessage;
using Messages.ServerMessage;

namespace OS_MatchTableServer.Services
{
    public sealed class MessageListener
    {
        private const string TcpIp = "127.0.0.34";
        private const int TcpPort = 12301;

        
        public MessageListener()
        {
            ThreadPool.QueueUserWorkItem(RunUdpClientService);
        }
        
        private async void RunUdpClientService(object? state)
        {
            using var udpSocket =
                new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) {EnableBroadcast = true};
            const string ip = "127.0.0.33";
            const int port = 12300;
            EndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            udpSocket.Bind(ipEndPoint);

            while (true)
            {
                var fullData = new List<byte>(128);
                EndPoint clientIpEndPoint = null!;
                do
                {
                    var buffer = new byte[128];

                    var bytesAmount = udpSocket.ReceiveFrom(buffer,SocketFlags.None,ref ipEndPoint);
                    var receivedBytes = buffer.Take(bytesAmount);
                    fullData.AddRange(receivedBytes);
                } while (udpSocket.Available > 0);

                var receivedMessage = HandleDataMessage(fullData.ToArray());
                if (receivedMessage != null)
                {
                    switch (receivedMessage)
                    {
                        case WhoIsServerMessage _:
                            var serverAddressMessage = new ServerAddressMessage
                            {
                                IP = TcpIp,
                                Port = TcpPort
                            };
                            var packedMessage = MessageConverter.PackMessage(serverAddressMessage);
                            await udpSocket.SendToAsync(packedMessage, SocketFlags.None,ipEndPoint);
                            break;
                    }
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private Message? HandleDataMessage(in byte[] data)
        {
            try
            {
                var unpackedMessage = MessageConverter.UnPackMessage(data);
                return unpackedMessage;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}