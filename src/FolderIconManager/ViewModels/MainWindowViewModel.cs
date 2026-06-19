using System.Windows.Input;
using FolderIconManager.Models;
using FolderIconManager.Services;

namespace FolderIconManager.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly IFolderIconService _folderIconService;
    private string _targetFolderPath = string.Empty;
    private string _iconPath = string.Empty;
    private string _statusText = "请选择目标文件夹和 ICO 图标。";
    private bool _isStatusSuccess;

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

    public ICommand ApplyCommand { get; }

    public ICommand RestoreCommand { get; }

    private void Apply() => ShowResult(_folderIconService.Apply(TargetFolderPath, IconPath));

    private void Restore() => ShowResult(_folderIconService.Restore(TargetFolderPath));

    private void ShowResult(OperationResult result)
    {
        StatusText = result.Message;
        IsStatusSuccess = result.IsSuccess;
    }
}
