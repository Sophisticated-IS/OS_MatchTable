using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MessageHandler;
using Messages.ClientMessage;
using Messages.ServerMessage;
using Messages.ServerMessage.Base;

namespace OS_MatchTableClient.Services
{
    public sealed class MessageSender
    {
        private IPEndPoint? _serverEndPoint;
        private Socket? _tcpSocket;
        private const string Ip = "127.0.0.66";
        private const int Port = 12666;

        public delegate void ServerSendMessageHandler(ServerMessage message);
        public event ServerSendMessageHandler ServerSendMessage = null!;
        public event EventHandler ServerIsDown = null!;
        
        // ReSharper disable once EmptyConstructor
        public MessageSender()
        {
            
        }


        public async Task StartListeningServer()
        {
            async Task<int?> TryReadSocketData(byte[] buffer)
            {
                int? readBytesAmount = null;
                try
                {
                    readBytesAmount = await _tcpSocket.ReceiveAsync(buffer, SocketFlags.None);
                }
                catch (SocketException)
                {
                }

                return readBytesAmount;
            }

            if (_tcpSocket is null) return;

            var isServerDown = false;
            while (!isServerDown)
            {
                var fullMessage = new List<byte>();
                do
                {
                    var buffer = new byte[1024];
                    var readBytesAmount = await TryReadSocketData(buffer);
                    if (!readBytesAmount.HasValue)
                    { 
                        ServerIsDown.Invoke(null,EventArgs.Empty);
                        isServerDown = true;
                        break;
                    }
                    
                    var readBytes = buffer.Take(readBytesAmount.Value);
                    fullMessage.AddRange(readBytes);
                } while (_tcpSocket.Available > 0);
                var unpackResult =   TryUnpackMessage(fullMessage,out var serverMessage);

                if (unpackResult)
                {
                    ServerSendMessage?.Invoke(serverMessage!);
                }

                
            }
        }

        private bool TryUnpackMessage(List<byte> fullMessage,out ServerMessage? serverMessage)
        {
            var isSuccess = false;
            serverMessage = null;
            try
            {
                var unpackedMessage = MessageConverter.UnPackMessage(fullMessage.ToArray());
                var castedServerMessage = (ServerMessage)unpackedMessage;
                serverMessage = castedServerMessage;
                isSuccess = true;
            }
            catch (Exception)
            {
                // ignored
            }

            return isSuccess;
        }

        public async Task<bool> ConnectToServer()
        {
            async Task<bool> TryConnectToServer(Socket socket)
            {
                var result = false;
                try
                {
                    await socket.ConnectAsync(_serverEndPoint ?? throw new InvalidOperationException());
                    result = true;
                }
                catch (Exception)
                {
                    // ignored
                }

                return result;
            }

            _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var serverIpEndPoint = new IPEndPoint(IPAddress.Parse(Ip), Port);
            _tcpSocket.Bind(serverIpEndPoint);
            var connectToServerResult = await TryConnectToServer(_tcpSocket);
            return connectToServerResult;
        }

        public async Task FindServer()
        {
            using var udpClient = new UdpClient();

            var whoIsServerMessage = new WhoIsServerMessage();
            var sendingMessageBytes = MessageConverter.PackMessage(whoIsServerMessage);

            bool isServerFound = false;
            while (!isServerFound)
            {
                const int port = 12300;
                var broadcastIpEndPoint = new IPEndPoint(IPAddress.Broadcast, port);
                await udpClient.SendAsync(sendingMessageBytes, sendingMessageBytes.Length, broadcastIpEndPoint);

                for (int i = 0; i < 2; i++)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    if (udpClient.Available > 0)
                    {
                        var receivedResult = await udpClient.ReceiveAsync();
                        isServerFound = UnpackServerAnswer(receivedResult);
                    }
                }
            }
        }

        private bool UnpackServerAnswer(UdpReceiveResult receivedResult)
        {
            bool isServerFound = false;
            try
            {
                var serverMessage = MessageConverter.UnPackMessage(receivedResult.Buffer);
                switch (serverMessage)
                {
                    case ServerAddressMessage serverAddress:
                        isServerFound = true;
                        _serverEndPoint = new IPEndPoint(IPAddress.Parse(serverAddress.IP), serverAddress.Port);
                        break;
                }
            }
            catch (Exception)
            {
                isServerFound = false;
            }

            return isServerFound;
        }

        ~MessageSender()
        {
            _tcpSocket?.Close();
            _tcpSocket?.Dispose();
        }
    }
}