using FolderIconManager.Services;

namespace FolderIconManager.Tests;

public sealed class DesktopIniEditorTests
{
    [Fact]
    public void RemoveIconResource_WritesEmptyIconResource()
    {
        using var workspace = new TestWorkspace();
        var folder = workspace.CreateDirectory("目标");
        var desktopIniPath = Path.Combine(folder, "desktop.ini");
        File.WriteAllText(
            desktopIniPath,
            "[.ShellClassInfo]\r\nIconResource=D:\\icons\\blue.ico,0\r\nInfoTip=保留\r\n");

        new DesktopIniEditor().RemoveIconResource(folder);

        var text = File.ReadAllText(desktopIniPath);
        Assert.True(File.Exists(desktopIniPath));
        Assert.Contains("[.ShellClassInfo]", text);
        Assert.Contains("InfoTip=保留", text);
        Assert.Contains("IconResource=,0", text);
    }

    [Fact]
    public void SetIconResource_PreservesOtherSectionsAndKeys()
    {
        using var workspace = new TestWorkspace();
        var folder = workspace.CreateDirectory("目标");
        var iconPath = workspace.CreateFile("蓝色.ico");
        var desktopIniPath = Path.Combine(folder, "desktop.ini");
        File.WriteAllText(
            desktopIniPath,
            "[ViewState]\r\nMode=\r\n[.ShellClassInfo]\r\nInfoTip=保留\r\n");

        new DesktopIniEditor().SetIconResource(folder, iconPath);

        var text = File.ReadAllText(desktopIniPath);
        Assert.Contains("[ViewState]\r\nMode=", text);
        Assert.Contains("InfoTip=保留", text);
        Assert.Contains($"IconResource={Path.GetFullPath(iconPath)},0", text);
    }

    [Fact]
    public void SetIconResource_MarksDesktopIniAsHiddenAndSystem()
    {
        using var workspace = new TestWorkspace();
        var folder = workspace.CreateDirectory("目标");
        var iconPath = workspace.CreateFile("蓝色.ico");

        new DesktopIniEditor().SetIconResource(folder, iconPath);

        var attributes = File.GetAttributes(Path.Combine(folder, "desktop.ini"));
        Assert.True((attributes & FileAttributes.Hidden) != 0);
        Assert.True((attributes & FileAttributes.System) != 0);
    }

    [Fact]
    public void SetIconResource_ReplacesExistingIconResourceOnHiddenSystemFile()
    {
        using var workspace = new TestWorkspace();
        var folder = workspace.CreateDirectory("目标");
        var firstIconPath = workspace.CreateFile("第一.ico");
        var secondIconPath = workspace.CreateFile("第二.ico");
        var editor = new DesktopIniEditor();
        editor.SetIconResource(folder, firstIconPath);

        editor.SetIconResource(folder, secondIconPath);

        var text = File.ReadAllText(Path.Combine(folder, "desktop.ini"));
        Assert.DoesNotContain(Path.GetFullPath(firstIconPath), text);
        Assert.Contains($"IconResource={Path.GetFullPath(secondIconPath)},0", text);
        Assert.Equal(
            1,
            File.ReadLines(Path.Combine(folder, "desktop.ini"))
                .Count(line => line.StartsWith("IconResource=", StringComparison.OrdinalIgnoreCase)));
    }
}
