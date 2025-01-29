using ChatClient.MVVM.Model;
using ChatClient.MVVM.Core;
using ChatClient.Net;
using System.Collections.ObjectModel;
using System.Windows;

namespace ChatClient.MVVM.ViewModel
{
    //[PropertyChanged]
    class MainViewModel : ObservableObject
    {
        public ObservableCollection<UserModel> Users { get; private set; }
        public ObservableCollection<ContactModel> Contacts { get; private set; }
        public ObservableCollection<MessageModel> Messages { get; private set; }
        public RelayCommand SendMessageCommand { get; set; }
        public RelayCommand ServerConnectCommand { get; set; }
        public string Username { get; set; }



        private ContactModel _selectedContact;

        private readonly Server _server = new Server();

        private string _message;

        public ContactModel SelectedContact
        {
            get { return _selectedContact; }
            set
            {
                _selectedContact = value;
                //SendMessageCommand.RaiseCanExecuteChanged();
                OnPropertyChanged();
            }
        }
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                //SendMessageCommand.RaiseCanExecuteChanged();
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            Users = new ObservableCollection<UserModel>();
            //Messages = new ObservableCollection<string>();
            Contacts = new ObservableCollection<ContactModel>();
            Messages = new ObservableCollection<MessageModel>();

            SendMessageCommand = new RelayCommand(
                async o =>
                {
                    if (!string.IsNullOrEmpty(Message))
                    {
                        await _server.SendMessageAsync(Message);
                        Message = string.Empty;
                    }
                },
                o => !string.IsNullOrWhiteSpace(Message));

            Messages.Add(new MessageModel
            {
                Username = "Bobby",
                ImageSource = "",
                Message = "Test",
                Time = DateTime.Now,
                IsOwnMessage = false,
                FirstMessage = true
            });

            for (int i = 0; i < 3; i++)
            {
                Messages.Add(new MessageModel
                {
                    Username = "Allison",
                    UsernameColor = "#509aef",
                    Message = "Test",
                    ImageSource = "/Resources/profile3.png",
                    Time = DateTime.Now,
                    IsOwnMessage = false,
                    FirstMessage = false
                });
            }

            for (int i = 0; i < 40; i++)
            {
                Messages.Add(new MessageModel
                {
                    Username = "Livv",
                    UsernameColor = "#ff3a51",
                    ImageSource = "/Resources/profile3.png",
                    Message = "Test",
                    Time = DateTime.Now,
                    IsOwnMessage = true,
                });
            }

            Messages.Add(new MessageModel
            {
                Username = "Borris",
                UsernameColor = "#409aff",
                ImageSource = "/Resources/profile3.png",
                Message = "Last",
                Time = DateTime.Now,
                IsOwnMessage = true,
            });

            for (int i = 0; i < 5; i++)
            {
                Contacts.Add(new ContactModel
                {
                    Username = $"Jermie {i}",
                    ImageSource = "./Resources/profile2.jpg",
                    Messages = Messages
                });
                Contacts.Add(new ContactModel
                {
                    Username = $"Joe {i}",
                    ImageSource = "Resources/profile1.jpg",
                    Messages = Messages
                });
                Contacts.Add(new ContactModel
                {
                    Username = $"Anne {i}",
                    ImageSource = "Resources/profile3.jpg",
                    Messages = Messages
                });
                Contacts.Add(new ContactModel
                {
                    Username = $"Lyle {i}",
                    ImageSource = "Resources/profile3.jpg",
                    Messages = Messages
                });
            }


            InitializeCommands();
            SubscribeToServerEvents();

        }

        public void InitializeConnection()
        {
            Task.Run(async () => await _server.ConnectToServerAsync(Username));
        }

        private void SubscribeToServerEvents()
        {
            _server.ConnectedEvent += async () => await UserConnected();
            _server.MessageReceivedEvent += (message) => MessageReceived(new MessageModel { Message = message });
            _server.DisconnectedEvent += RemoveUser;
        }

        private void InitializeCommands()
        {
            ServerConnectCommand = new RelayCommand(
                async o => await _server.ConnectToServerAsync(Username),
                o => true);
            System.Diagnostics.Debug.WriteLine("ConnectToServerAsync has been invoked.");


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

        private async void RemoveUser()
        {
            var uid = await _server.PacketReader.ReadStringAsync();
            var user = Users.FirstOrDefault(x => x.UID == uid);
            Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
        }

        private void MessageReceived(MessageModel message)
        {
            Application.Current.Dispatcher.Invoke(() => Messages.Add(message));
        }

        private async Task UserConnected()
        {
            var user = new UserModel
            {
                Username = await _server.PacketReader.ReadStringAsync(),
                UID = await _server.PacketReader.ReadStringAsync()
            };

            if (!Users.Any(x => x.UID == user.UID))
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(user));
            }

        }
    }
}
