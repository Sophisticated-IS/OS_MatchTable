using System;

using System.Timers;
using Avalonia.Media;
using OS_MatchTableClient.Services;
using OS_MatchTableClient.ViewModels.Base;
using ReactiveUI;
using Timer = System.Timers.Timer;

namespace OS_MatchTableClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private StatusConnection _connectionStatus;
        private ISolidColorBrush? _connectionStatusColor;
        private readonly MessageSender _messageSender;
        private Timer _timer;
        private TimeSpan _matchTime;

        public enum StatusConnection
        {
            Disconnected,
            Connected
        }

        public TimeSpan MatchTime
        {
            get => _matchTime;
            private set =>this.RaiseAndSetIfChanged(ref _matchTime, value);
        }

        public StatusConnection ConnectionStatus
        {
            get => _connectionStatus;
            private set => this.RaiseAndSetIfChanged(ref _connectionStatus, value);
        }

        public ISolidColorBrush? ConnectionStatusColor
        {
            get => _connectionStatusColor;
            private set => this.RaiseAndSetIfChanged(ref _connectionStatusColor, value);
        }


        public MainWindowViewModel()
        {
            _messageSender = new MessageSender();
            this.WhenAnyValue(vm => vm.ConnectionStatus).Subscribe(_ => UpdateConnectionStatusColor());
            ConnectionStatus = StatusConnection.Disconnected;
            _timer = new Timer
            {
                Interval = TimeSpan.FromSeconds(1).TotalMilliseconds
            };
            _timer.Elapsed += TimerOnElapsed;
            ConnectToServer();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            MatchTime = MatchTime.Add(TimeSpan.FromSeconds(1));
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
            await _messageSender.FindServer();
            ConnectionStatus = StatusConnection.Connected;
            var connectionServerResult = await _messageSender.ConnectToServer();
            if (connectionServerResult)
            {
                _timer.Start();
            }
            
        }
        
        
    }
}