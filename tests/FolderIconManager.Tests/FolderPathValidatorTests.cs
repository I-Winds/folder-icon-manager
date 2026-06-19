using FolderIconManager.Services;

namespace FolderIconManager.Tests;

public sealed class FolderPathValidatorTests
{
    [Fact]
    public void ValidateIcon_RejectsEmptyPath()
    {
        var result = new FolderPathValidator().ValidateIcon(null);

        Assert.False(result.IsSuccess);
        Assert.Equal("未选择 ICO 文件。", result.Message);
    }

    [Fact]
    public void ValidateIcon_RejectsNonIcoExtension()
    {
        using var workspace = new TestWorkspace();
        var textFile = workspace.CreateFile("图标.txt");

        var result = new FolderPathValidator().ValidateIcon(textFile);

        Assert.False(result.IsSuccess);
        Assert.Equal("请选择 .ico 图标文件。", result.Message);
    }

    [Fact]
    public void ValidateIcon_RejectsMissingIcoFile()
    {
        using var workspace = new TestWorkspace();
        var missingIcon = Path.Combine(workspace.RootPath, "不存在.ico");

        var result = new FolderPathValidator().ValidateIcon(missingIcon);

        Assert.False(result.IsSuccess);
        Assert.Equal("ICO 文件不存在。", result.Message);
    }

    [Fact]
    public void ValidateIcon_AcceptsExistingIcoFile()
    {
        using var workspace = new TestWorkspace();
        var iconFile = workspace.CreateFile("图标.ico");

        var result = new FolderPathValidator().ValidateIcon(iconFile);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateTarget_RejectsEmptyPath()
    {
        var result = new FolderPathValidator().ValidateTarget(null);

        Assert.False(result.IsSuccess);
        Assert.Equal("未选择目标文件夹。", result.Message);
    }

    [Fact]
    public void ValidateTarget_RejectsMissingFolder()
    {
        using var workspace = new TestWorkspace();
        var missingFolder = Path.Combine(workspace.RootPath, "不存在");

        var result = new FolderPathValidator().ValidateTarget(missingFolder);

        Assert.False(result.IsSuccess);
        Assert.Equal("目标文件夹不存在。", result.Message);
    }

    [Fact]
    public void ValidateTarget_RejectsUncPathBeforeCheckingWhetherFolderExists()
    {
        var result = new FolderPathValidator().ValidateTarget(@"\\服务器\共享\目标");

        Assert.False(result.IsSuccess);
        Assert.Equal("不支持网络目录。", result.Message);
    }

    [Fact]
    public void ValidateTarget_AcceptsExistingNonCDriveDirectory()
    {
        using var workspace = new TestWorkspace();
        var folder = workspace.CreateDirectory("目标");

        var result = new FolderPathValidator().ValidateTarget(folder);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateTarget_RejectsCDriveBeforeCheckingWhetherFolderExists()
    {
        var path = "C:" + Path.DirectorySeparatorChar + "任意目录";

        var result = new FolderPathValidator().ValidateTarget(path);

        Assert.False(result.IsSuccess);
        Assert.Equal("为了避免影响系统盘和系统目录，本工具不支持修改 C 盘文件夹图标。", result.Message);
    }
}
