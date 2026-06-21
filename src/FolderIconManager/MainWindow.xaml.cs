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
    private static readonly Brush DropBackgroundBrush = new SolidColorBrush(Color.FromRgb(239, 246, 255));
    private static readonly Brush DefaultPathBackgroundBrush = new SolidColorBrush(Color.FromRgb(248, 250, 252));

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

    private void PathTextBox_PreviewDragEnter(object sender, DragEventArgs e)
    {
        UpdateDragState(sender as TextBox, e);
    }

    private void PathTextBox_PreviewDragOver(object sender, DragEventArgs e)
    {
        UpdateDragState(sender as TextBox, e);
    }

    private void PathTextBox_PreviewDragLeave(object sender, DragEventArgs e)
    {
        ResetDragState(sender as TextBox);
        e.Handled = true;
    }

    private void TargetFolderPathTextBox_Drop(object sender, DragEventArgs e)
    {
        ResetDragState(sender as TextBox);

        if (TryGetSingleDroppedPath(e, out var path) && System.IO.Directory.Exists(path))
        {
            ViewModel.TargetFolderPath = path;
        }
        else
        {
            ViewModel.ShowDropError("请选择一个存在的文件夹。");
        }

        e.Handled = true;
    }

    private void IconPathTextBox_Drop(object sender, DragEventArgs e)
    {
        ResetDragState(sender as TextBox);

        if (TryGetSingleDroppedPath(e, out var path) &&
            System.IO.File.Exists(path) &&
            string.Equals(System.IO.Path.GetExtension(path), ".ico", StringComparison.OrdinalIgnoreCase))
        {
            ViewModel.IconPath = path;
        }
        else
        {
            ViewModel.ShowDropError("请选择一个存在的 ICO 文件。");
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

    private void UpdateDragState(TextBox? textBox, DragEventArgs e)
    {
        if (textBox is null)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        var hasFileDrop = e.Data.GetDataPresent(DataFormats.FileDrop);
        var acceptsDrop = TryGetSingleDroppedPath(e, out var path) &&
            ((textBox == TargetFolderPathTextBox && System.IO.Directory.Exists(path)) ||
             (textBox == IconPathTextBox && System.IO.File.Exists(path) &&
              string.Equals(System.IO.Path.GetExtension(path), ".ico", StringComparison.OrdinalIgnoreCase)));

        e.Effects = hasFileDrop ? DragDropEffects.Copy : DragDropEffects.None;
        if (acceptsDrop)
        {
            textBox.Background = DropBackgroundBrush;
            textBox.BorderBrush = (Brush)FindResource("AccentBrush");
        }
        else
        {
            ResetDragState(textBox);
        }

        e.Handled = true;
    }

    private void ResetDragState(TextBox? textBox)
    {
        if (textBox is null)
        {
            return;
        }

        textBox.Background = DefaultPathBackgroundBrush;
        textBox.BorderBrush = (Brush)FindResource("BorderBrush");
    }
}
