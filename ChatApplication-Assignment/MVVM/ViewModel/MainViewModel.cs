using ChatClient.MVVM.Model;
using ChatClient.MVVM.Core;
using ChatClient.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ChatClient.MVVM.ViewModel
{
    //[PropertyChanged]
    class MainViewModel
    {
        public ObservableCollection<ContactModel> Contacts { get; private set; } 
        public ObservableCollection<string> Messages { get; private set; }
        public RelayCommand SendMessageCommand { get; set; }
        public RelayCommand ServerConnectCommand { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }

        private readonly Server _server = new Server();


        public MainViewModel()
        {
            InitializeCommands();
            SubscribeToServerEvents();
        }

        private void InitializeCommands()
        {
            throw new NotImplementedException();
        }
    }
}
