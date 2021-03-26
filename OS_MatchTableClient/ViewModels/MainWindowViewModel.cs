using System;
using Avalonia.Media;
using OS_MatchTableClient.Services;
using OS_MatchTableClient.ViewModels.Base;
using ReactiveUI;

namespace OS_MatchTableClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private StatusConnection _connectionStatus;
        private ISolidColorBrush _connectionStatusColor;
        private readonly MessageSender _messageSender;

        public enum StatusConnection
        {
            Disconnected,
            Connected
        }
        
        public StatusConnection ConnectionStatus
        {
            get => _connectionStatus;
            private set => this.RaiseAndSetIfChanged(ref _connectionStatus, value);
        }

        public ISolidColorBrush ConnectionStatusColor
        {
            get => _connectionStatusColor;
            private set => this.RaiseAndSetIfChanged(ref _connectionStatusColor, value);
        }


        public MainWindowViewModel()
        {
            _messageSender = new MessageSender();
            this.WhenAnyValue(vm => vm.ConnectionStatus).Subscribe(_ => UpdateConnectionStatusColor());
            ConnectionStatus = StatusConnection.Disconnected;
            ConnectToServer();
        }

        private void UpdateConnectionStatusColor()
        {
            ConnectionStatusColor = ConnectionStatus switch
            {
                StatusConnection.Disconnected => Brushes.Red,
                StatusConnection.Connected => Brushes.Green,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        
        private async void ConnectToServer()
        {
            await _messageSender.SetConnectionWithServer();
            ConnectionStatus = StatusConnection.Connected;
        }
    }
}