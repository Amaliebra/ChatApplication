using System.Collections.ObjectModel;

namespace ChatClient.MVVM.Model
{
    class ContactModel
    {
        public string Username { get; set; }
        public string ImageSource { get; set; }
        public ObservableCollection<MessageModel> Messages { get; set; } = new ObservableCollection<MessageModel>();
        public string LastMessage => Messages.LastOrDefault()?.Message ?? "No messages yet";

        public string UID { get; set; }
    }
}
