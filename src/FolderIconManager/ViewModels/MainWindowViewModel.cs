using System.Windows.Input;
using System.IO;
using FolderIconManager.Models;
using FolderIconManager.Services;

namespace FolderIconManager.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly IFolderIconService _folderIconService;
    private string _targetFolderPath = string.Empty;
    private string _iconPath = string.Empty;
    private string _statusText = "等待操作：请选择目标文件夹和 ICO 图标。";
    private bool _isStatusSuccess;
    private bool _isStatusError;

    public MainWindowViewModel(IFolderIconService folderIconService)
    {
        _folderIconService = folderIconService;
        ApplyCommand = new RelayCommand(Apply);
        RestoreCommand = new RelayCommand(Restore);
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

    public ICommand ApplyCommand { get; }

    public ICommand RestoreCommand { get; }

    public void ShowDropError(string message)
    {
        StatusText = $"拖入失败：{message}";
        IsStatusSuccess = false;
        IsStatusError = true;
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
}
