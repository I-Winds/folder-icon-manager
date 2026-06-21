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
    private string _statusTitle = "等待操作";
    private string _statusDetail = "请选择目标文件夹和 ICO 图标。";
    private string _statusTime = string.Empty;
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

    public string StatusTitle
    {
        get => _statusTitle;
        private set => SetProperty(ref _statusTitle, value);
    }

    public string StatusDetail
    {
        get => _statusDetail;
        private set => SetProperty(ref _statusDetail, value);
    }

    public string StatusTime
    {
        get => _statusTime;
        private set => SetProperty(ref _statusTime, value);
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

    private void Apply()
    {
        var iconName = Path.GetFileName(IconPath);
        ShowProcessing("正在应用图标", string.IsNullOrEmpty(iconName) ? "正在检查 ICO 图标。" : iconName);
        ShowResult(
            _folderIconService.Apply(TargetFolderPath, IconPath),
            "图标已应用",
            "应用失败",
            string.IsNullOrEmpty(iconName) ? "已应用自定义图标。" : iconName);
    }

    private void Restore()
    {
        ShowProcessing("正在恢复默认图标", "正在移除自定义图标。" );
        ShowResult(
            _folderIconService.Restore(TargetFolderPath),
            "已恢复默认图标",
            "恢复失败",
            "已移除自定义图标。");
    }

    private void ShowProcessing(string title, string detail)
    {
        StatusTitle = title;
        StatusDetail = detail;
        StatusTime = string.Empty;
        IsStatusSuccess = false;
        IsStatusError = false;
    }

    private void ShowResult(
        OperationResult result,
        string successTitle,
        string errorTitle,
        string successDetail)
    {
        StatusTitle = result.IsSuccess ? successTitle : errorTitle;
        StatusDetail = result.IsSuccess ? successDetail : result.Message;
        StatusTime = DateTime.Now.ToString("HH:mm:ss");
        IsStatusSuccess = result.IsSuccess;
        IsStatusError = !result.IsSuccess;
    }
}
