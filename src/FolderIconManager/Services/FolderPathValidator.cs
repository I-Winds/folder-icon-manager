using System.IO;
using FolderIconManager.Models;

namespace FolderIconManager.Services;

public sealed class FolderPathValidator
{
    private readonly string _desktopDirectory = Path.TrimEndingDirectorySeparator(
        Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)));

    public OperationResult ValidateIcon(string? iconPath)
    {
        if (string.IsNullOrWhiteSpace(iconPath))
        {
            return new OperationResult(false, "未选择 ICO 文件。");
        }

        if (!string.Equals(Path.GetExtension(iconPath), ".ico", StringComparison.OrdinalIgnoreCase))
        {
            return new OperationResult(false, "请选择 .ico 图标文件。");
        }

        if (!File.Exists(iconPath))
        {
            return new OperationResult(false, "ICO 文件不存在。");
        }

        return new OperationResult(true, "ICO 文件有效。");
    }

    public OperationResult ValidateTarget(string? folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            return new OperationResult(false, "未选择目标文件夹。");
        }

        var fullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(folderPath));

        if (Uri.TryCreate(fullPath, UriKind.Absolute, out var uri) && uri.IsUnc)
        {
            return new OperationResult(false, "不支持网络目录。");
        }

        if (IsCDrivePath(fullPath) && !IsDescendantOfDesktop(fullPath))
        {
            return new OperationResult(false, "C 盘仅支持修改桌面文件夹及其子文件夹。");
        }

        if (!Directory.Exists(fullPath))
        {
            return new OperationResult(false, "目标文件夹不存在。");
        }

        return new OperationResult(true, "目标文件夹有效。");
    }

    private static bool IsCDrivePath(string fullPath)
    {
        return string.Equals(Path.GetPathRoot(fullPath), @"C:\", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsDescendantOfDesktop(string fullPath)
    {
        var relativePath = Path.GetRelativePath(_desktopDirectory, fullPath);
        return !string.Equals(relativePath, ".", StringComparison.Ordinal) &&
               !string.Equals(relativePath, "..", StringComparison.Ordinal) &&
               !relativePath.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
               !relativePath.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal) &&
               !Path.IsPathRooted(relativePath);
    }
}
