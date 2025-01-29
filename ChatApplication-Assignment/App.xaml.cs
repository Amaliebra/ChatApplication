using ChatClient.MVVM.View;
using ChatClient.MVVM.ViewModel;
using System.Configuration;
using System.Data;
using System.Windows;

namespace ChatApplication_Assignment
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            System.Diagnostics.Debug.WriteLine("Application starting - showing loginview");

            var loginWindow = new LoginView { DataContext = new LoginViewModel() };
            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();
        }
    }

}
