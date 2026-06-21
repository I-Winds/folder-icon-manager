using System.Text;
using System.Windows;
using FolderIconManager.Services;
using FolderIconManager.ViewModels;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FolderIconManager;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(
            new FolderIconService(new FolderPathValidator(), new DesktopIniEditor()));
    }

    private void ChooseFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog();
        if (dialog.ShowDialog(this) == true)
        {
            ViewModel.TargetFolderPath = dialog.FolderName;
        }
    }

    private void ChooseIcon_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "ICO 图标 (*.ico)|*.ico",
            Multiselect = false
        };

        if (dialog.ShowDialog(this) == true)
        {
            ViewModel.IconPath = dialog.FileName;
        }
    }
}
