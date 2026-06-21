using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using FolderIconManager.Models;
using FolderIconManager.Services;

namespace FolderIconManager.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly IFolderIconService _folderIconService;
    private readonly FavoriteStore _favoriteStore;
    private string _targetFolderPath = string.Empty;
    private string _iconPath = string.Empty;
    private string _statusText = "等待操作：请选择目标文件夹和 ICO 图标。";
    private bool _isStatusSuccess;
    private bool _isStatusError;
    private FavoriteFolderItem? _selectedFavoriteFolder;
    private FavoriteIconItem? _selectedFavoriteIcon;

    public MainWindowViewModel(
        IFolderIconService folderIconService,
        FavoriteStore favoriteStore)
    {
        _folderIconService = folderIconService;
        _favoriteStore = favoriteStore;
        ApplyCommand = new RelayCommand(Apply);
        RestoreCommand = new RelayCommand(Restore);
        LoadFavorites();
    }

    public string TargetFolderPath
    {
        get => _targetFolderPath;
        set => SetProperty(ref _targetFolderPath, value);
    }

    public string IconPath
    {
        get => _iconPath;
        set => SetProperty(ref _iconPath, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public bool IsStatusSuccess
    {
        get => _isStatusSuccess;
        private set => SetProperty(ref _isStatusSuccess, value);
    }

    public bool IsStatusError
    {
        get => _isStatusError;
        private set => SetProperty(ref _isStatusError, value);
    }

    public ObservableCollection<FavoriteFolderItem> FavoriteFolders { get; } = [];

    public FavoriteFolderItem? SelectedFavoriteFolder
    {
        get => _selectedFavoriteFolder;
        private set
        {
            if (SetProperty(ref _selectedFavoriteFolder, value))
            {
                OnPropertyChanged(nameof(FavoritePreviewDescription));
            }
        }
    }

    public FavoriteIconItem? SelectedFavoriteIcon
    {
        get => _selectedFavoriteIcon;
        set
        {
            if (SetProperty(ref _selectedFavoriteIcon, value))
            {
                if (value is not null)
                {
                    IconPath = value.IconPath;
                }

                OnPropertyChanged(nameof(FavoritePreviewDescription));
                OnPropertyChanged(nameof(CanOpenSelectedFavoriteIcon));
            }
        }
    }

    public string FavoritePreviewDescription => SelectedFavoriteFolder is not null && SelectedFavoriteIcon is not null
        ? $"{SelectedFavoriteFolder.DisplayName} / {SelectedFavoriteIcon.FileName}"
        : string.Empty;

    public bool CanOpenSelectedFavoriteIcon => SelectedFavoriteIcon is not null && File.Exists(SelectedFavoriteIcon.IconPath);

    public ICommand ApplyCommand { get; }

    public ICommand RestoreCommand { get; }

    public void AddFavoriteFolder(string folderPath)
    {
        try
        {
            var fullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(folderPath));
            if (!Directory.Exists(fullPath))
            {
                ShowFavoriteError("收藏夹目录不存在。");
                return;
            }

            var existingFolder = FavoriteFolders.FirstOrDefault(folder =>
                string.Equals(folder.FolderPath, fullPath, StringComparison.OrdinalIgnoreCase));
            if (existingFolder is not null)
            {
                ExpandFavoriteFolder(existingFolder);
                ShowFavoriteSuccess($"已选中收藏夹：{existingFolder.DisplayName}");
                return;
            }

            var folderPaths = FavoriteFolders
                .Select(folder => folder.FolderPath)
                .Append(fullPath)
                .ToList();
            _favoriteStore.Save(folderPaths);

            var favoriteFolder = new FavoriteFolderItem(fullPath);
            FavoriteFolders.Add(favoriteFolder);
            ExpandFavoriteFolder(favoriteFolder);
            ShowFavoriteSuccess($"已添加收藏夹：{favoriteFolder.DisplayName}");
        }
        catch (Exception)
        {
            ShowFavoriteError("无法保存收藏夹数据。");
        }
    }

    public void ExpandFavoriteFolder(FavoriteFolderItem favoriteFolder)
    {
        if (!FavoriteFolders.Contains(favoriteFolder))
        {
            return;
        }

        foreach (var folder in FavoriteFolders)
        {
            folder.IsExpanded = ReferenceEquals(folder, favoriteFolder);
        }

        SelectedFavoriteFolder = favoriteFolder;
        SelectedFavoriteIcon = null;
        LoadFavoriteIcons(favoriteFolder);
    }

    public void CollapseFavoriteFolder(FavoriteFolderItem favoriteFolder)
    {
        favoriteFolder.IsExpanded = false;
        if (!ReferenceEquals(SelectedFavoriteFolder, favoriteFolder))
        {
            return;
        }

        SelectedFavoriteFolder = null;
        SelectedFavoriteIcon = null;
    }

    public void RemoveFavoriteFolder(FavoriteFolderItem favoriteFolder)
    {
        var index = FavoriteFolders.IndexOf(favoriteFolder);
        if (index < 0)
        {
            return;
        }

        try
        {
            var remainingPaths = FavoriteFolders
                .Where(folder => !ReferenceEquals(folder, favoriteFolder))
                .Select(folder => folder.FolderPath)
                .ToList();
            _favoriteStore.Save(remainingPaths);

            FavoriteFolders.Remove(favoriteFolder);
            var nextIndex = Math.Min(index, FavoriteFolders.Count - 1);
            if (nextIndex >= 0)
            {
                ExpandFavoriteFolder(FavoriteFolders[nextIndex]);
            }
            else
            {
                SelectedFavoriteFolder = null;
                SelectedFavoriteIcon = null;
            }
            ShowFavoriteSuccess($"已从收藏夹移除：{favoriteFolder.DisplayName}");
        }
        catch (Exception)
        {
            ShowFavoriteError("无法保存收藏夹数据。");
        }
    }

    public void ApplyFavoriteIcon(FavoriteIconItem favoriteIcon)
    {
        IconPath = favoriteIcon.IconPath;
        Apply();
    }

    public void OpenSelectedFavoriteIconLocation()
    {
        var iconPath = SelectedFavoriteIcon?.IconPath;
        if (string.IsNullOrWhiteSpace(iconPath) || !File.Exists(iconPath))
        {
            ShowFavoriteError("ICO 图标文件不存在。");
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{iconPath}\"")
            {
                UseShellExecute = true
            });
            ShowFavoriteSuccess($"已定位图标：{Path.GetFileName(iconPath)}");
        }
        catch (Exception)
        {
            ShowFavoriteError("无法打开 ICO 所在文件夹。");
        }
    }

    private void Apply()
    {
        var iconName = Path.GetFileName(IconPath);
        var displayName = string.IsNullOrEmpty(iconName) ? "ICO 图标" : iconName;
        ShowProcessing($"正在应用图标：{displayName}");
        ShowResult(
            _folderIconService.Apply(TargetFolderPath, IconPath),
            $"图标已应用：{displayName}",
            "应用失败");
    }

    private void Restore()
    {
        ShowProcessing("正在恢复默认图标");
        ShowResult(
            _folderIconService.Restore(TargetFolderPath),
            "已恢复默认图标",
            "恢复失败");
    }

    private void ShowProcessing(string statusText)
    {
        StatusText = statusText;
        IsStatusSuccess = false;
        IsStatusError = false;
    }

    private void ShowResult(
        OperationResult result,
        string successText,
        string errorTitle)
    {
        StatusText = result.IsSuccess ? successText : $"{errorTitle}：{result.Message}";
        IsStatusSuccess = result.IsSuccess;
        IsStatusError = !result.IsSuccess;
    }

    private void LoadFavorites()
    {
        try
        {
            foreach (var folderPath in _favoriteStore.Load()
                         .Where(path => !string.IsNullOrWhiteSpace(path))
                         .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                FavoriteFolders.Add(new FavoriteFolderItem(folderPath));
            }

            if (FavoriteFolders.FirstOrDefault() is { } firstFolder)
            {
                ExpandFavoriteFolder(firstFolder);
            }
        }
        catch (Exception)
        {
            ShowFavoriteError("无法读取收藏夹数据。");
        }
    }

    private void LoadFavoriteIcons(FavoriteFolderItem favoriteFolder)
    {
        favoriteFolder.Icons.Clear();

        try
        {
            if (!Directory.Exists(favoriteFolder.FolderPath))
            {
                ShowFavoriteError("收藏夹目录不存在。");
                return;
            }

            foreach (var iconPath in Directory
                         .EnumerateFiles(favoriteFolder.FolderPath, "*.ico", SearchOption.TopDirectoryOnly)
                         .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase))
            {
                favoriteFolder.Icons.Add(new FavoriteIconItem(iconPath));
            }
        }
        catch (Exception)
        {
            ShowFavoriteError("无法读取收藏夹中的 ICO 图标。");
        }
    }

    private void ShowFavoriteSuccess(string message)
    {
        StatusText = message;
        IsStatusSuccess = true;
        IsStatusError = false;
    }

    private void ShowFavoriteError(string message)
    {
        StatusText = message;
        IsStatusSuccess = false;
        IsStatusError = true;
    }
}
