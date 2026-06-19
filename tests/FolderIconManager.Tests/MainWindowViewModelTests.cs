using FolderIconManager.Models;
using FolderIconManager.Services;
using FolderIconManager.ViewModels;

namespace FolderIconManager.Tests;

public sealed class MainWindowViewModelTests
{
    [Fact]
    public void RestoreCommand_ExposesSuccessMessageInStatusArea()
    {
        var viewModel = new MainWindowViewModel(
            new StubFolderIconService(new OperationResult(false, "未选择目标文件夹。")));

        viewModel.RestoreCommand.Execute(null);

        Assert.Equal("已恢复默认图标。", viewModel.StatusText);
        Assert.True(viewModel.IsStatusSuccess);
    }

    [Fact]
    public void ApplyCommand_ExposesFailureMessageInStatusArea()
    {
        var viewModel = new MainWindowViewModel(
            new StubFolderIconService(new OperationResult(false, "未选择目标文件夹。")));

        viewModel.ApplyCommand.Execute(null);

        Assert.Equal("未选择目标文件夹。", viewModel.StatusText);
        Assert.False(viewModel.IsStatusSuccess);
    }

    private sealed class StubFolderIconService : IFolderIconService
    {
        private readonly OperationResult _applyResult;

        public StubFolderIconService(OperationResult applyResult)
        {
            _applyResult = applyResult;
        }

        public OperationResult Apply(string? folderPath, string? iconPath) => _applyResult;

        public OperationResult Restore(string? folderPath) => new OperationResult(true, "已恢复默认图标。");
    }
}
