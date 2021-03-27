using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using JetBrains.Annotations;
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
        
        private BlockingCollection<Socket> _clientsConnections;
        
        public MessageListener()
        {
            _clientsConnections = new BlockingCollection<Socket>();
            ThreadPool.QueueUserWorkItem(RunTcpClientAccepting);
            ThreadPool.QueueUserWorkItem(RunUdpClientService);
        }

        private async void RunTcpClientAccepting(object? state)
        {
            var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var serverIpEndPoint = new IPEndPoint(IPAddress.Parse(TcpIp), TcpPort);
            tcpSocket.Bind(serverIpEndPoint);
            tcpSocket.Listen(100);
            while (true)
            {
                var clientConnection = await tcpSocket.AcceptAsync();
                _clientsConnections.Add(clientConnection);
                // ThreadPool.QueueUserWorkItem(StartListenClientConnection,clientConnection);   
            }
        }

        // private async void StartListenClientConnection([NotNull] object? state)
        // {
        //     if (state == null) return;
        //     var clientConnection = (Socket) state;
        //     
        //     try
        //     {
        //
        //         clientConnection.Listen(1);
        //     
        //         await clientConnection.rece
        //     }
        //     catch (Exception)
        //     { 
        //         _clientsConnections.TryTake(out clientConnection);
        //         clientConnection?.Close();
        //     }
        //     //todo
        //     
        // }

        private async void RunUdpClientService(object? state)
        {
            var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            const int port = 12300;
            EndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, port);
            udpSocket.Bind(ipEndPoint);

            while (true)
            {
                var fullData = new List<byte>(128);
                EndPoint clientIpEndPoint = new IPEndPoint(IPAddress.None, 0);
                do
                {
                    var buffer = new byte[128];

                    var bytesAmount = udpSocket.ReceiveFrom(buffer, SocketFlags.None, ref clientIpEndPoint);
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
                            await udpSocket.SendToAsync(packedMessage, SocketFlags.None, clientIpEndPoint);
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