
using ChatClient.MVVM.Core;

namespace ChatClient.MVVM.Model
{
    class MessageModel : ObservableObject
    {
        public string Username { get; set; }
        public string UsernameColor { get; set; }
        public string Message { get; set; }
        public string ImageSource { get; set; }
        public DateTime Time { get; set; }

        private bool _firstMessage;
        private bool _lastMessage;
        private bool _isOwnMessage;

        public bool FirstMessage
        {
            get => _firstMessage;
            set
            {
                _firstMessage = value;
                OnPropertyChanged();
            }
        }

        public bool LastMessage
        {
            get => _lastMessage;
            set
            {
                _lastMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsOwnMessage
        {
            get => _isOwnMessage;
            set
            {
                _isOwnMessage = value;
                OnPropertyChanged();
            }

        }
    }
}
