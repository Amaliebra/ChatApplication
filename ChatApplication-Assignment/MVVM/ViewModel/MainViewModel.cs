using ChatClient.MVVM.Model;
using ChatClient.MVVM.Core;
using ChatClient.Net;
using System.Collections.ObjectModel;
using System.Windows;
using ChatClient.Net.IO;

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

        private string _defaultColor = "#8a6130";
        private string _ownColor = "#56C54C";

        public string UsernameColor
        {
            get
            {
                if (SelectedContact == null) return _defaultColor;

                return (SelectedContact.Username == Username) ? _ownColor : _defaultColor;
            }
        }

        private ContactModel _selectedContact;

        private readonly Server _server = new Server();

        private string _message;
        private bool _isConnected = false;

        public ContactModel SelectedContact
        {
            get { return _selectedContact; }
            set
            {
                _selectedContact = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedContact.Messages));
                OnPropertyChanged(nameof(UsernameColor)); 
            }
        }
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel(string username)
        {
            System.Diagnostics.Debug.WriteLine("MainViewModel called");

            Username = username ?? "DefaultUser";
            Users = new ObservableCollection<UserModel>();
            Contacts = new ObservableCollection<ContactModel>();
            Messages = new ObservableCollection<MessageModel>();

            _server.ConnectedEvent += UserConnected;
            _server.UserListUpdatedEvent += OnUserListUpdated;
            _server.ConnectToServerAsync(Username);

            InitializeCommands();
            SubscribeToServerEvents();
            System.Diagnostics.Debug.WriteLine("Starting connection");
            InitializeConnection();

        }

        public void InitializeConnection()
        {
            if (_isConnected)
            {
                System.Diagnostics.Debug.WriteLine("Already connected. Skipping duplicate connection.");
                return;
            }

            _isConnected = true;

            System.Diagnostics.Debug.WriteLine($"Initializing connection for {Username}...");
            Task.Run(async () =>
            {
                System.Diagnostics.Debug.WriteLine($"Attempting to connect as {Username}");
                await _server.ConnectToServerAsync(Username);
                System.Diagnostics.Debug.WriteLine($"Connection request sent for {Username}");
            });
        }

        private void OnUserListUpdated(List<string> userList)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] OnUserListUpdated CALLED!");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] User list updated. Previous SelectedContact: {SelectedContact?.Username ?? "NULL"}");

                //if(!userList.Contains(Username))
                //{
                //    userList.Add(Username);
                //}

                //var PreviousContact = SelectedContact?.Username;
                //var ExistingContacts = Contacts.ToDictionary(c => c.Username);
                var ExistingUsernames = Contacts.Select(c => c.Username).ToList();

                //Users.Clear(); //check if this removes the user list
                //Contacts.Clear();

                foreach (var user in userList)
                {
                    if (!ExistingUsernames.Contains(user))
                    {
                        Console.WriteLine();
                    }

                        Contacts.Add(new ContactModel
                        {
                            Username = user,
                            Messages = new ObservableCollection<MessageModel>(),
                        });

                }
                var PreviousContact = SelectedContact?.Username;

                //SelectedContact = Contacts.FirstOrDefault(c => c.Username == PreviousContact);

                //System.Diagnostics.Debug.WriteLine($"[DEBUG] After update, SelectedContact: {SelectedContact?.Username ?? "NULL"}");

                if (PreviousContact != null)
                {
                    SelectedContact = Contacts.FirstOrDefault(c => c.Username == PreviousContact);
                    if (SelectedContact == null)
                    {
                        System.Diagnostics.Debug.WriteLine("[WARNING] SelectedContact was lost after user list update, even with update logic!");
                    }
                }
                //if (SelectedContact == null)
                //{
                //    System.Diagnostics.Debug.WriteLine("[WARNING] SelectedContact was lost after user list update!");
                //}
                System.Diagnostics.Debug.WriteLine($"[DEBUG] After update, SelectedContact: {SelectedContact?.Username ?? "NULL"}");
            });
        }

        //private void OnUserListUpdated(List<string> userList)
        //{
        //    Application.Current.Dispatcher.Invoke(() =>
        //    {
        //        foreach (var contact in Contacts)
        //        {
        //            contact.IsOnline = userList.Contains(contact.Username);
        //        }
        //    });
        //}


        private void SubscribeToServerEvents()
        {
            _server.MessageReceivedEvent += (message) => Application.Current.Dispatcher.Invoke(() =>
            {
                if (SelectedContact != null)
                {
                    SelectedContact.Messages.Add(new MessageModel
                    {
                        Username = "Other",  
                        Message = message,
                        Time = DateTime.Now,
                        IsOwnMessage = false
                    });

                    System.Diagnostics.Debug.WriteLine($"Message received and added to UI: {message}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No contact selected. Message not displayed");
                }
            });
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
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Sending message. SelectedContact: {SelectedContact?.Username ?? "NULL"}");

                    if (!string.IsNullOrWhiteSpace(Message) && SelectedContact != null)
                    {
                        var packetBuilder = new PacketBuilder();
                        packetBuilder.WriteOpCode(5);
                        packetBuilder.WriteString(SelectedContact.Username);
                        packetBuilder.WriteString(Message);

                        await _server.SendMessageAsync(packetBuilder.GetPacketBytes());

                        SelectedContact.Messages.Add(new MessageModel
                        {
                            Username = Username,
                            Message = Message,
                            Time = DateTime.Now,
                            IsOwnMessage = true
                        });

                        Message = string.Empty;

                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Message sent. SelectedContact: {SelectedContact?.Username ?? "NULL"}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[ERROR] Cannot send message. SelectedContact: {SelectedContact?.Username ?? "NULL"}");
                    }
                },
                o => !string.IsNullOrWhiteSpace(Message) && SelectedContact != null);

        }

        //private async void RemoveUser()
        //{
        //    var uid = await _server.PacketReader.ReadStringAsync();
        //    var user = Users.FirstOrDefault(x => x.UID == uid);
        //    Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
        //}

        //private void MessageReceived(MessageModel message)
        //{
        //    Application.Current.Dispatcher.Invoke(() =>
        //    {
        //        Messages.Add(message);

        //        var contact = Contacts.FirstOrDefault(c => c.Username == message.Username);
        //        if (contact != null)
        //        {
        //            contact.Messages.Add(message);
        //        }
        //    });
        //}

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
