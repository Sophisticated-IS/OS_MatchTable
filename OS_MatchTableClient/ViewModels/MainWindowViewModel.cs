using System;
using System.Collections.ObjectModel;
using System.Timers;
using Avalonia.Media;
using Messages.ServerMessage;
using Messages.ServerMessage.Base;
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
        private readonly Timer _timer;
        private TimeSpan _matchTime;
        private uint _rogersScore;
        private uint _bulletsScore;

        public enum StatusConnection
        {
            Disconnected,
            Connected,
            ServerIsDown
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


        public uint RogersScore
        {
            get => _rogersScore;
            set =>this.RaiseAndSetIfChanged(ref _rogersScore, value);
        }

        public uint BulletsScore
        {
            get => _bulletsScore;
            set => this.RaiseAndSetIfChanged(ref _bulletsScore,value);
        }

        public ObservableCollection<string> MatchEvents { get; }
        public MainWindowViewModel()
        {
            _messageSender = new MessageSender();
            MatchEvents = new ObservableCollection<string>();
            this.WhenAnyValue(vm => vm.ConnectionStatus).Subscribe(_ => UpdateConnectionStatusColor());
            ConnectionStatus = StatusConnection.Disconnected;
            _timer = new Timer
            {
                Interval = TimeSpan.FromSeconds(1).TotalMilliseconds
            };
            _timer.Elapsed += TimerOnElapsed;
            ConnectToServerAsync();
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
                StatusConnection.ServerIsDown => Brushes.Orange,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        
        private async void ConnectToServerAsync()
        {
            await _messageSender.FindServerAsync();
            ConnectionStatus = StatusConnection.Connected;
            var connectionServerResult = await _messageSender.ConnectToServerAsync();
            if (connectionServerResult)
            {
                _timer.Start();
                _messageSender.ServerSendMessage += MessageSenderOnServerSendMessage;
                _messageSender.ServerIsDown += MessageSenderOnServerIsDown;
                await _messageSender.StartListeningServerAsync();
            }
            
        }

        private void MessageSenderOnServerIsDown(object? sender, EventArgs e)
        {
            ConnectionStatus = StatusConnection.ServerIsDown;
            _timer?.Stop();
        }

        private void MessageSenderOnServerSendMessage(ServerMessage message)
        {

            switch (message)
            {
                case GoalMessage goalMessage:
                    var matchEvent = $"Player: {goalMessage.Player} in Team: {goalMessage.Team} scored a goal! Congratulations!!!";
                    MatchEvents.Add(matchEvent);
                    switch (goalMessage.Team)
                    {
                        case "Rogers":
                            BulletsScore++;
                            break;
                        case "Bullets":
                            RogersScore++;
                            break;
                    }

                    break;
                
            }
        }
    }
}