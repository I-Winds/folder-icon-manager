using FolderIconManager.Services;

namespace FolderIconManager.Tests;

public sealed class FolderIconServiceTests
{
    [Fact]
    public void ShellNotifier_RefreshFolder_DoesNotThrowForExistingDirectory()
    {
        using var workspace = new TestWorkspace();
        var folder = workspace.CreateDirectory("目标");

        var exception = Record.Exception(() => new ShellNotifier().RefreshFolder(folder));

        Assert.Null(exception);
    }

    [Fact]
    public void Apply_MapsUnauthorizedAccessToShortStatusMessage()
    {
        using var workspace = new TestWorkspace();
        var folder = workspace.CreateDirectory("目标");
        var iconPath = workspace.CreateFile("蓝色.ico");
        var service = new FolderIconService(
            new FolderPathValidator(),
            new ThrowingDesktopIniEditor(),
            new RecordingShellNotifier());

        var result = service.Apply(folder, iconPath);

        Assert.False(result.IsSuccess);
        Assert.Equal("没有写入权限。", result.Message);
    }

    [Fact]
    public void Apply_MapsIoFailureToShortStatusMessage()
    {
        using var workspace = new TestWorkspace();
        var folder = workspace.CreateDirectory("目标");
        var iconPath = workspace.CreateFile("蓝色.ico");
        var service = new FolderIconService(
            new FolderPathValidator(),
            new IoFailingDesktopIniEditor(),
            new RecordingShellNotifier());

        var result = service.Apply(folder, iconPath);

        Assert.False(result.IsSuccess);
        Assert.Equal("写入 desktop.ini 失败。", result.Message);
    }

    [Fact]
    public void Apply_MapsShellRefreshFailureToShortStatusMessage()
    {
        using var workspace = new TestWorkspace();
        var folder = workspace.CreateDirectory("目标");
        var iconPath = workspace.CreateFile("蓝色.ico");
        var service = new FolderIconService(
            new FolderPathValidator(),
            new DesktopIniEditor(),
            new ThrowingShellNotifier());

        var result = service.Apply(folder, iconPath);

        Assert.False(result.IsSuccess);
        Assert.Equal("Shell 刷新失败或未能立即生效。", result.Message);
    }

    [Fact]
    public void Restore_RemovesIconResourceAndNotifiesShell()
    {
        using var workspace = new TestWorkspace();
        var folder = workspace.CreateDirectory("目标");
        var iconPath = workspace.CreateFile("蓝色.ico");
        var editor = new DesktopIniEditor();
        editor.SetIconResource(folder, iconPath);
        var notifier = new RecordingShellNotifier();
        var service = new FolderIconService(new FolderPathValidator(), editor, notifier);

        var result = service.Restore(folder);

        Assert.True(result.IsSuccess);
        Assert.Equal("已恢复默认图标，已通知资源管理器刷新。", result.Message);
        Assert.DoesNotContain("IconResource=", File.ReadAllText(Path.Combine(folder, "desktop.ini")));
        Assert.Equal(folder, notifier.LastRefreshedFolder);
    }

    [Fact]
    public void Apply_UsesOriginalIconPathMarksFolderAndNotifiesShell()
    {
        using var workspace = new TestWorkspace();
        var folder = workspace.CreateDirectory("目标");
        var iconPath = workspace.CreateFile("蓝色.ico");
        var notifier = new RecordingShellNotifier();
        var service = new FolderIconService(new FolderPathValidator(), new DesktopIniEditor(), notifier);

        var result = service.Apply(folder, iconPath);

        Assert.True(result.IsSuccess);
        Assert.Equal("图标已应用，已通知资源管理器刷新。", result.Message);
        Assert.Contains(
            $"IconResource={Path.GetFullPath(iconPath)},0",
            File.ReadAllText(Path.Combine(folder, "desktop.ini")));
        Assert.False(File.Exists(Path.Combine(folder, Path.GetFileName(iconPath))));
        Assert.Equal(folder, notifier.LastRefreshedFolder);
        Assert.True((File.GetAttributes(folder) & FileAttributes.System) != 0);
    }

    private sealed class RecordingShellNotifier : IShellNotifier
    {
        public string? LastRefreshedFolder { get; private set; }

        public void RefreshFolder(string folderPath)
        {
            LastRefreshedFolder = folderPath;
        }
    }

    private sealed class ThrowingDesktopIniEditor : IDesktopIniEditor
    {
        public void SetIconResource(string folderPath, string iconPath)
        {
            throw new UnauthorizedAccessException();
        }

        public void RemoveIconResource(string folderPath)
        {
            throw new UnauthorizedAccessException();
        }
    }

    private sealed class IoFailingDesktopIniEditor : IDesktopIniEditor
    {
        public void SetIconResource(string folderPath, string iconPath)
        {
            throw new IOException();
        }

        public void RemoveIconResource(string folderPath)
        {
            throw new IOException();
        }
    }

    private sealed class ThrowingShellNotifier : IShellNotifier
    {
        public void RefreshFolder(string folderPath)
        {
            throw new System.Runtime.InteropServices.ExternalException();
        }
    }
}
