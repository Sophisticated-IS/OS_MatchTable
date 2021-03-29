using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        public string[] Teams
        {
            get => _teams;
            set => this.RaiseAndSetIfChanged(ref _teams, value);
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
            FillMatchData();
            _startServerDateTime = DateTime.Now;
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
            Teams = new[] {"Rogers", "Bullets"};

            var rogersTeam = new[]
            {
                "Bill(1)",
                "Roger(2)",
                "David(3)",
                "Mary(4)",
                "Lily(5)",
                "Bro(6)",
                "Nice(7)",
                "Kelvin(8)",
                "Brown(9)",
                "Noland(10)",
                "Bilden(11)",
            };
            var bulletsTeam = new[]
            {
                "Shoun(1)",
                "Golem(2)",
                "Holand(3)",
                "Ferry(4)",
                "Filiska(5)",
                "Pipilom(6)",
                "Kolen(7)",
                "Nordan(8)",
                "Duland(9)",
                "Mortran(10)",
                "Rick(11)"
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