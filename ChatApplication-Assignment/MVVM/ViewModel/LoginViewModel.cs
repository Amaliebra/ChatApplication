using ChatClient.MVVM.Core;
using System.Windows;
using ChatClient.MVVM.View;


namespace ChatClient.MVVM.ViewModel
{
    class LoginViewModel : ObservableObject
    {
        public string Username { get; set; }
        public string Password { get; set; } = "password";
        public RelayCommand LoginCommand { get; set; }

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

        public LoginViewModel()
        {
            System.Diagnostics.Debug.WriteLine("LoginViewModel called");

            var random = new Random();
            Username = _usernames[random.Next(_usernames.Count)];
            System.Diagnostics.Debug.WriteLine($"default username: {Username}");

            LoginCommand = new RelayCommand(o => Login());

        }
        private void Login()
        {
            System.Diagnostics.Debug.WriteLine($"Attempting login as {Username}");

            if (Application.Current.Windows.OfType<MainWindow>().Any())
            {
                System.Diagnostics.Debug.WriteLine("MainView open, skipping instance");
                return;
            }

            var mainViewModel = new MainViewModel { Username = this.Username };

            var mainWindow = new MainWindow { DataContext = mainViewModel };

            mainViewModel.InitializeConnection();

            Application.Current.Dispatcher.Invoke(() =>
            {
                mainWindow.Show();

                foreach (Window window in Application.Current.Windows)
                {
                    if (window is LoginView)
                    {
                        window.Close();
                        break;
                    }
                }
            });
        }
    }

}