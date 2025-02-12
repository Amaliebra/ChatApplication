using ChatClient.MVVM.ViewModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChatClient.MVVM.Model;

namespace ChatClient;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Border_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }

    }
    private void ChatBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return)
        {
            if (DataContext is MainViewModel viewModel && viewModel.SendMessageCommand.CanExecute(null))
            {
                viewModel.SendMessageCommand.Execute(null);
                e.Handled = true;
            }
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.MainWindow.WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        if(Application.Current.MainWindow.WindowState != WindowState.Maximized)
        {
            Application.Current.MainWindow.WindowState = WindowState.Maximized;
        }
        else
        {
            Application.Current.MainWindow.WindowState = WindowState.Normal;
        }
    }

    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            if (e.AddedItems.Count > 0)
            {
                viewModel.SelectedContact = e.AddedItems[0] as ContactModel;
                System.Diagnostics.Debug.WriteLine($"[DEBUG] SelectedContact set to: {viewModel.SelectedContact?.Username}");
            }
        }
    }

}