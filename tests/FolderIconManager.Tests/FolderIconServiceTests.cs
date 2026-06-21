using FolderIconManager.Services;

namespace FolderIconManager.Tests;

public sealed class FolderIconServiceTests
{
    [Fact]
    public void Apply_MapsUnauthorizedAccessToShortStatusMessage()
    {
        using var workspace = new TestWorkspace();
        var folder = workspace.CreateDirectory("目标");
        var iconPath = workspace.CreateFile("蓝色.ico");
        var service = new FolderIconService(
            new FolderPathValidator(),
            new ThrowingDesktopIniEditor());

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
            new IoFailingDesktopIniEditor());

        var result = service.Apply(folder, iconPath);

        Assert.False(result.IsSuccess);
        Assert.Equal("写入 desktop.ini 失败。", result.Message);
    }

    [Fact]
    public void Apply_MapsShellApiFailureToShortStatusMessage()
    {
        using var workspace = new TestWorkspace();
        var folder = workspace.CreateDirectory("目标");
        var iconPath = workspace.CreateFile("蓝色.ico");
        var service = new FolderIconService(
            new FolderPathValidator(),
            new ThrowingExternalDesktopIniEditor());

        var result = service.Apply(folder, iconPath);

        Assert.False(result.IsSuccess);
        Assert.Equal("Shell 设置文件夹图标失败。", result.Message);
    }

    [Fact]
    public void Restore_WritesEmptyIconResource()
    {
        using var workspace = new TestWorkspace();
        var folder = workspace.CreateDirectory("目标");
        var iconPath = workspace.CreateFile("蓝色.ico");
        var editor = new DesktopIniEditor();
        editor.SetIconResource(folder, iconPath);
        var service = new FolderIconService(new FolderPathValidator(), editor);

        var result = service.Restore(folder);

        Assert.True(result.IsSuccess);
        Assert.Equal("已恢复默认图标。", result.Message);
        Assert.Contains("IconResource=,0", File.ReadAllText(Path.Combine(folder, "desktop.ini")));
    }

    [Fact]
    public void Apply_UsesOriginalIconPathAndMarksFolder()
    {
        using var workspace = new TestWorkspace();
        var folder = workspace.CreateDirectory("目标");
        var iconPath = workspace.CreateFile("蓝色.ico");
        var service = new FolderIconService(new FolderPathValidator(), new DesktopIniEditor());

        var result = service.Apply(folder, iconPath);

        Assert.True(result.IsSuccess);
        Assert.Equal("图标已应用。", result.Message);
        Assert.Contains(
            $"IconResource={Path.GetFullPath(iconPath)},0",
            File.ReadAllText(Path.Combine(folder, "desktop.ini")));
        Assert.False(File.Exists(Path.Combine(folder, Path.GetFileName(iconPath))));
        Assert.True((File.GetAttributes(folder) & FileAttributes.System) != 0);
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

    private sealed class ThrowingExternalDesktopIniEditor : IDesktopIniEditor
    {
        public void SetIconResource(string folderPath, string iconPath)
        {
            throw new System.Runtime.InteropServices.ExternalException();
        }

        public void RemoveIconResource(string folderPath)
        {
            throw new System.Runtime.InteropServices.ExternalException();
        }
    }
}
