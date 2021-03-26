using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MessageHandler;
using Messages.ClientMessage;
using Messages.ServerMessage;

namespace OS_MatchTableClient.Services
{
    public sealed class MessageSender
    {
        private IPEndPoint? _serverEndPoint;

        public MessageSender()
        {
        }

        public async Task SetConnectionWithServer()
        {
            using var udpClient = new UdpClient("127.0.0.33",12300) {EnableBroadcast = true};

            var whoIsServerMessage = new WhoIsServerMessage();
            var sendingMessageBytes = MessageConverter.PackMessage(whoIsServerMessage);

            bool isServerFound = false;
            while (!isServerFound)
            {
                await udpClient.SendAsync(sendingMessageBytes, sendingMessageBytes.Length);

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
    }
}