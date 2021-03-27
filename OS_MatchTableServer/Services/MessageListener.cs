using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MessageHandler;
using Messages.Base;
using Messages.ClientMessage;
using Messages.ServerMessage;
using Messages.ServerMessage.Base;

namespace OS_MatchTableServer.Services
{
    public sealed class MessageListener
    {
        private const string TcpIp = "127.0.0.34";
        private const int TcpPort = 12301;
        private Socket? _tcpSocket;

        private List<Socket> _clientsConnections;
        private object _lockerClientsConnections;

        public MessageListener()
        {
            _clientsConnections = new List<Socket>();
            _lockerClientsConnections = new object();
            ThreadPool.QueueUserWorkItem(RunTcpClientAccepting);
            ThreadPool.QueueUserWorkItem(RunUdpClientService);
        }

        private async void RunTcpClientAccepting(object? state)
        {
            _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var serverIpEndPoint = new IPEndPoint(IPAddress.Parse(TcpIp), TcpPort);
            _tcpSocket.Bind(serverIpEndPoint);
            _tcpSocket.Listen(100);
            while (true)
            {
                var clientConnection = await _tcpSocket.AcceptAsync();
                lock (_lockerClientsConnections)
                {
                    _clientsConnections.Add(clientConnection);
                }
            }

            // ReSharper disable once FunctionNeverReturns
        }

        public async Task SendMessage(ServerMessage serverMessage)
        {
            Socket[] GetLocalClientConnections()
            {
                Socket[] localClientConnections1;
                lock (_lockerClientsConnections)
                {
                    localClientConnections1 = _clientsConnections.ToArray();
                }

                return localClientConnections1;
            }

            void RemoveDisconnectedClients(List<Socket> sockets)
            {
                for (var i = 0; i < sockets.Count; i++)
                {
                    var clientSocket = sockets[i];
                    lock (_lockerClientsConnections)
                    {
                        _clientsConnections.Remove(clientSocket);
                    }
                }
            }

            async Task<bool> TrySendServerMessage(byte[] bytes,Socket clientConnection)
            {
                var result = false;
                try
                {
                    await clientConnection.SendAsync(bytes, SocketFlags.None);
                    result = true;
                }
                catch (Exception)
                {
                    // ignored
                }

                return result;
            }

            var sendingMessage = MessageConverter.PackMessage(serverMessage);
            var disconnectedClients = new List<Socket>();
            var localClientConnections = GetLocalClientConnections();
            foreach (var clientConnection in localClientConnections)
            {
                var sendMessageResult = await TrySendServerMessage(sendingMessage,clientConnection);
                if (!sendMessageResult)
                {
                    disconnectedClients.Add(clientConnection);
                }
            }

            RemoveDisconnectedClients(disconnectedClients);
        }
   
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