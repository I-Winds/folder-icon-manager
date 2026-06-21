using System.Text;
using System.Windows;
using FolderIconManager.Models;
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
    private bool _isFavoritePanelExpanded = true;
    private bool _isFavoriteSidebarExpanded = true;

    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(
            new FolderIconService(new FolderPathValidator(), new DesktopIniEditor()),
            new FavoriteStore());
    }

    private void ChooseFolder_Click(object sender, RoutedEventArgs e)
    {
        SelectTargetFolder();
    }

    private void SelectTargetFolder()
    {
        var dialog = new OpenFolderDialog();
        if (dialog.ShowDialog(this) == true)
        {
            ViewModel.TargetFolderPath = dialog.FolderName;
        }
    }

    private void ChooseIcon_Click(object sender, RoutedEventArgs e)
    {
        SelectIconFile();
    }

    private void SelectIconFile()
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

    private void TargetFolderDropZone_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!IsInteractiveInput(e.OriginalSource))
        {
            SelectTargetFolder();
            e.Handled = true;
        }
    }

    private void IconDropZone_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!IsInteractiveInput(e.OriginalSource))
        {
            SelectIconFile();
            e.Handled = true;
        }
    }

    private static bool IsInteractiveInput(object source)
    {
        for (var element = source as FrameworkElement; element is not null; element = element.Parent as FrameworkElement)
        {
            if (element is Button or TextBox)
            {
                return true;
            }
        }

        return false;
    }

    private void AddFavorite_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog();
        if (dialog.ShowDialog(this) == true)
        {
            ViewModel.AddFavoriteFolder(dialog.FolderName);
        }
    }

    private void RemoveFavoriteFolder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { CommandParameter: FavoriteFolderItem favoriteFolder })
        {
            ViewModel.RemoveFavoriteFolder(favoriteFolder);
        }
    }

    private void ToggleFavoritePanel_Click(object sender, RoutedEventArgs e)
    {
        _isFavoritePanelExpanded = !_isFavoritePanelExpanded;
        FavoritePanelContent.Visibility = _isFavoritePanelExpanded ? Visibility.Visible : Visibility.Collapsed;
        FavoritePanelRow.Height = _isFavoritePanelExpanded
            ? new GridLength(1.35, GridUnitType.Star)
            : new GridLength(46);
        FavoritePanelToggleIcon.Text = _isFavoritePanelExpanded ? "\uE70D" : "\uE70E";
    }

    private void FavoriteSidebarHandle_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isFavoriteSidebarExpanded = !_isFavoriteSidebarExpanded;
        FavoriteSidebarContent.Visibility = _isFavoriteSidebarExpanded ? Visibility.Visible : Visibility.Collapsed;
        FavoriteSidebarColumn.Width = new GridLength(_isFavoriteSidebarExpanded ? 220 : 0);
        FavoriteSidebarGap.Width = new GridLength(8);
        FavoriteDropZone.CornerRadius = _isFavoriteSidebarExpanded
            ? new CornerRadius(6, 0, 0, 6)
            : new CornerRadius(6);
        FavoriteSidebarHandle.CornerRadius = _isFavoriteSidebarExpanded
            ? new CornerRadius(0, 6, 6, 0)
            : new CornerRadius(6);
        FavoriteSidebarChevron.Data = Geometry.Parse(_isFavoriteSidebarExpanded
            ? "M 5,10 L 2,14 L 5,18 Z"
            : "M 3,10 L 6,14 L 3,18 Z");
        FavoriteSidebarHandle.ToolTip = _isFavoriteSidebarExpanded ? "收起图标收藏" : "展开图标收藏";
        e.Handled = true;
    }

    private void FavoriteFolderExpander_Expanded(object sender, RoutedEventArgs e)
    {
        if (sender is Expander { DataContext: FavoriteFolderItem favoriteFolder })
        {
            ViewModel.ExpandFavoriteFolder(favoriteFolder);
        }
    }

    private void FavoriteFolderExpander_Collapsed(object sender, RoutedEventArgs e)
    {
        if (sender is Expander { DataContext: FavoriteFolderItem favoriteFolder })
        {
            ViewModel.CollapseFavoriteFolder(favoriteFolder);
        }
    }

    private void OpenSelectedFavoriteIconLocation_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.OpenSelectedFavoriteIconLocation();
    }

    private void FavoriteIcons_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListBox { SelectedItem: FavoriteIconItem favoriteIcon })
        {
            ViewModel.ApplyFavoriteIcon(favoriteIcon);
        }
    }

    private void FavoriteDropZone_PreviewDragEnter(object sender, DragEventArgs e)
    {
        UpdateFavoriteDropZoneState(sender as Border, e);
    }

    private void FavoriteDropZone_PreviewDragOver(object sender, DragEventArgs e)
    {
        UpdateFavoriteDropZoneState(sender as Border, e);
    }

    private void FavoriteDropZone_PreviewDragLeave(object sender, DragEventArgs e)
    {
        ResetDropZoneState(sender as Border);
        e.Handled = true;
    }

    private void FavoriteDropZone_Drop(object sender, DragEventArgs e)
    {
        ResetDropZoneState(sender as Border);

        if (TryGetSingleDroppedPath(e, out var path) && System.IO.Directory.Exists(path))
        {
            ViewModel.AddFavoriteFolder(path);
        }

        e.Handled = true;
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
            (((dropZone == TargetFolderDropZone || dropZone == NewTargetFolderDropZone) && System.IO.Directory.Exists(path)) ||
             ((dropZone == IconDropZone || dropZone == NewIconDropZone) && System.IO.File.Exists(path) &&
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

    private void UpdateFavoriteDropZoneState(Border? dropZone, DragEventArgs e)
    {
        if (dropZone is null)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        var acceptsDrop = TryGetSingleDroppedPath(e, out var path) && System.IO.Directory.Exists(path);
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
