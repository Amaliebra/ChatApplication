using ChatClient.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.MVVM.ViewModel
{
    class MainViewModel
    {
        public ObservableCollection<ContactModel> Contacts { get; set; }
        public ObservableCollection<MessageModel> Messages { get; set; }

        public MainViewModel()
        {
            Messages = new ObservableCollection<MessageModel>();
            Contacts = new ObservableCollection<ContactModel>();

            Messages.Add(new MessageModel
            {
                Username = "FrenchPost",
                ImageSource = "Resources/profile3.png",
                UsernameColor = "Blue",
                Message = "Fortnite??? kidding! this is a test",
                Time = DateTime.Now,
                IsOwnMessage = false,
                FirstMessage = true
            });

            for (int i = 0; i < 3; i++)
            {
                Messages.Add(new MessageModel
                {
                    Username = "DonaldrrRrr",
                    ImageSource = "Resources/profile2.png",
                    UsernameColor = "Red",
                    Message = "Fortnite??? kidding! this is a test",
                    Time = DateTime.Now,
                    IsOwnMessage = false,
                    FirstMessage = false

                });
            }
            Messages.Add(new MessageModel
            {
                Username = "AdmiralStoli",
                ImageSource = "Resources/profile1.png",
                UsernameColor = "Red",
                Message = "Fortnite??? kidding! this is a test",
                Time = DateTime.Now,
                IsOwnMessage = true,
                FirstMessage = false

            });

            for (int i = 0; i < 3; i++)
            {
                Contacts.Add(new ContactModel
                {
                    Username = $"FrenchPost{i}",
                    ImageSource = "Resources/profile1.png",
                    Messages = Messages
                });
            }

        }

    }
}
