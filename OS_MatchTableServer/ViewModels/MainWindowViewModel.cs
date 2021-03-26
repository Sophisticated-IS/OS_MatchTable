using OS_MatchTableServer.Services;

namespace OS_MatchTableServer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private MessageListener _messageListener;

        public MainWindowViewModel()
        {
            _messageListener = new MessageListener();
        }

       
    }
}