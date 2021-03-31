using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Timers;
using DynamicData;
using Messages.ServerMessage;
using OS_MatchTableServer.Services;
using ReactiveUI;

namespace OS_MatchTableServer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        // ReSharper disable once NotAccessedField.Local
        private MessageListener _messageListener;
        private List<string[]> _teamPlayers;
        private string[] _teams;
        private string[] _players;
        private string _selectedTeam;
        private string _selectedPlayer;
        private DateTime _startServerDateTime;
        private Timer _timer;
        private TimeSpan _serverTimer;

        public string[] Teams
        {
            get => _teams;
            set => this.RaiseAndSetIfChanged(ref _teams, value);
        }

        public TimeSpan ServerTimer
        {
            get => _serverTimer;
            set => this.RaiseAndSetIfChanged(ref _serverTimer, value);
        }

        public string[] Players
        {
            get => _players;
            set => this.RaiseAndSetIfChanged(ref _players, value);
        }

        public string SelectedTeam
        {
            get => _selectedTeam;
            set => this.RaiseAndSetIfChanged(ref _selectedTeam, value);
        }

        public string SelectedPlayer
        {
            get => _selectedPlayer;
            set => this.RaiseAndSetIfChanged(ref _selectedPlayer, value);
        }

        // public ReactiveCommand<string, Unit> GoalCommand { get; }

        public MainWindowViewModel()
        {
            this.WhenAnyValue(vm => vm.SelectedTeam).Subscribe(UpdatePlayers);
            _teamPlayers = new List<string[]>(2);
            _messageListener = new MessageListener();
            _timer = new Timer()
            {
                Interval = TimeSpan.FromSeconds(1).TotalMilliseconds
            };
            _timer.Elapsed+= TimerOnElapsed;
            _timer.Start();
                FillMatchData();
            _startServerDateTime = DateTime.Now;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            ServerTimer = ServerTimer.Add(TimeSpan.FromSeconds(1));
        }

        public async void Goal(string parameter)
        {
            var goalTime = DateTime.Now.Subtract(_startServerDateTime);
            TruncatetimeSpanToSeconds(ref goalTime);
            var goalMessage = new GoalMessage
            {
                Team = SelectedTeam,
                Player = SelectedPlayer,
                ServerTime = goalTime
            };
            await _messageListener.SendMessage(goalMessage);
        }

        private static void TruncatetimeSpanToSeconds(ref TimeSpan timeSpan)
        {
            var time = timeSpan.ToString();
            var excessTicks = time.Split('.').First();
            var truncatedTimeSpan = DateTime.ParseExact(excessTicks, "HH:mm:ss", CultureInfo.InvariantCulture).TimeOfDay;
            timeSpan = truncatedTimeSpan;
        }

        private bool CanGoal(object parameter)
        {
            return SelectedTeam != null && SelectedPlayer != null;
        }

        private void FillMatchData()
        {
            Teams = new[] {"Команда№1", "Команда№2"};

            var rogersTeam = new[]
            {
                "Аршавин#1",
                "Аршавин#2",
                "Аршавин#3",
                "Аршавин#4",
                "Аршавин#5",
            };
            var bulletsTeam = new[]
            {
                "Мейси#1",
                "Мейси#2",
                "Мейси#3",
                "Мейси#4",
                "Мейси#5",
            };
            _teamPlayers.Add(rogersTeam);
            _teamPlayers.Add(bulletsTeam);

            SelectedTeam = Teams.First();
            Players = _teamPlayers.First();
        }

        private void UpdatePlayers(string? team)
        {
            if (team is null) return;

            var index = _teams.IndexOf(team);
            Players = _teamPlayers[index];
            SelectedPlayer = Players.First();
        }
    }
}