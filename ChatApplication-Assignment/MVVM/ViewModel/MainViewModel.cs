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
    class MainViewModel : ObservableObject
    {
        public ObservableCollection<UserModel> Users { get; private set; }
        public ObservableCollection<ContactModel> Contacts { get; private set; }
        public ObservableCollection<string> Messages { get; private set; }
        public RelayCommand SendMessageCommand { get; set; }
        public RelayCommand ServerConnectCommand { get; set; }
        public string Username { get; set; }

        public ContactModel SelectedContact { get; set; }

        private readonly Server _server = new Server();

        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                SendMessageCommand.RaiseCanExecuteChanged();
            }
        }

        private readonly List<string> _usernames = new()
        {
            "xXGamer360NoScopeXx",
            "CoolCat123",
            "N00b_Destroyer_1337",
            "I_H4t3_L1f3_xD",
            "xX_Slay3r_Xx",
            "SkullzOnF1r3",
            "Xx_D3thStalker_xX",
            "Pwnz0r",
            "xXx_1337_xXx",
            "R4venBl8de",
            "D3athM4ch1n3",
            "AKSprayL0rd",
            "Pray_N_Spray",
        };

        public MainViewModel()
        {
            Users = new ObservableCollection<UserModel>();
            Messages = new ObservableCollection<string>();
            Contacts = new ObservableCollection<ContactModel>();

            var random = new Random();
            Username = _usernames[random.Next(0, _usernames.Count)];
            System.Diagnostics.Debug.WriteLine($"Hi {Username}, how is your day going?");

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
                Username = _server.PacketReader.ReadStringAsync(),
                UID = _server.PacketReader.ReadString()
            };

            if (!Users.Any(x => x.UID == user.UID))
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(user));
            }

        }
    }
}
