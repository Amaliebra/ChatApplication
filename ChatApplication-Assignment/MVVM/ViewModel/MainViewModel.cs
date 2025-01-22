using ChatClient.MVVM.Model;
using ChatClient.MVVM.Core;
using ChatClient.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace ChatClient.MVVM.ViewModel
{
    //[PropertyChanged]
    class MainViewModel
    {
        public ObservableCollection<UserModel> Users { get; private set; }
        public ObservableCollection<ContactModel> Contacts { get; private set; } 
        public ObservableCollection<string> Messages { get; private set; }
        public RelayCommand SendMessageCommand { get; set; }
        public RelayCommand ServerConnectCommand { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }

        private readonly Server _server = new Server();


        public MainViewModel()
        {
            Users = new ObservableCollection<UserModel>();
            Messages = new ObservableCollection<string>();
            Contacts = new ObservableCollection<ContactModel>();

            InitializeCommands();
            SubscribeToServerEvents();
        }

        private void SubscribeToServerEvents()
        {
            _server.ConnectedEvent += UserConnected;
            _server.MessageReceivedEvent += MessageReceived;
            _server.DisconnectedEvent += RemoveUser;
        }

        private void InitializeCommands()
        {
            Username = "xXGamer360NoScopeXx";
            ServerConnectCommand = new RelayCommand(
                async o => await _server.ConnectToServerAsync(Username),
                o => true);



            SendMessageCommand = new RelayCommand(
                async o =>
                {
                    if(!string.IsNullOrEmpty(Message))
                    {
                        await _server.SendMessageAsync(Message);
                        Message = string.Empty;
                    }
                },
                o => !string.IsNullOrWhiteSpace(Message));
        }

        private void RemoveUser()
        {
            var uid = _server.PacketReader.ReadString();
            var user = Users.FirstOrDefault(x => x.UID == uid);
            Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
        }

        private void MessageReceived(string message)
        {
            Application.Current.Dispatcher.Invoke(() => Messages.Add(message));
        }

        private async Task UserConnected()
        {
            var user = new UserModel
            {
                //Username = _server.PacketReader.ReadStringAsync(),
                UID = _server.PacketReader.ReadString()
            };

            if (!Users.Any(x => x.UID == user.UID))
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(user));
            }

        }
    }
}
