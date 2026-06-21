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

    private void DropZone_PreviewDragEnter(object sender, DragEventArgs e)
    {
        UpdateDropZoneState(sender as Border, e);
    }

    private void DropZone_PreviewDragOver(object sender, DragEventArgs e)
    {
        UpdateDropZoneState(sender as Border, e);
    }

    private void DropZone_PreviewDragLeave(object sender, DragEventArgs e)
    {
        ResetDropZoneState(sender as Border);
        e.Handled = true;
    }

    private void TargetFolderDropZone_Drop(object sender, DragEventArgs e)
    {
        ResetDropZoneState(sender as Border);

        if (TryGetSingleDroppedPath(e, out var path) && System.IO.Directory.Exists(path))
        {
            ViewModel.TargetFolderPath = path;
        }

        e.Handled = true;
    }

    private void IconDropZone_Drop(object sender, DragEventArgs e)
    {
        ResetDropZoneState(sender as Border);

        if (TryGetSingleDroppedPath(e, out var path) &&
            System.IO.File.Exists(path) &&
            string.Equals(System.IO.Path.GetExtension(path), ".ico", StringComparison.OrdinalIgnoreCase))
        {
            ViewModel.IconPath = path;
        }

        e.Handled = true;
    }

    private static bool TryGetSingleDroppedPath(DragEventArgs e, out string path)
    {
        path = string.Empty;
        if (!e.Data.GetDataPresent(DataFormats.FileDrop) ||
            e.Data.GetData(DataFormats.FileDrop) is not string[] paths ||
            paths.Length != 1 ||
            string.IsNullOrWhiteSpace(paths[0]))
        {
            return false;
        }

        path = paths[0];
        return true;
    }

    private void UpdateDropZoneState(Border? dropZone, DragEventArgs e)
    {
        if (dropZone is null)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        var acceptsDrop = TryGetSingleDroppedPath(e, out var path) &&
            ((dropZone == TargetFolderDropZone && System.IO.Directory.Exists(path)) ||
             (dropZone == IconDropZone && System.IO.File.Exists(path) &&
              string.Equals(System.IO.Path.GetExtension(path), ".ico", StringComparison.OrdinalIgnoreCase)));

        e.Effects = acceptsDrop ? DragDropEffects.Copy : DragDropEffects.None;
        if (acceptsDrop)
        {
            dropZone.Background = (Brush)FindResource("DropZoneActiveBrush");
            dropZone.BorderBrush = (Brush)FindResource("AccentBrush");
        }
        else
        {
            ResetDropZoneState(dropZone);
        }

        e.Handled = true;
    }

    private void ResetDropZoneState(Border? dropZone)
    {
        if (dropZone is null)
        {
            return;
        }

        dropZone.Background = (Brush)FindResource("DropZoneBrush");
        dropZone.BorderBrush = (Brush)FindResource("BorderBrush");
    }
}
