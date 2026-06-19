using System.IO;
using FolderIconManager.Models;

namespace FolderIconManager.Services;

public sealed class FolderPathValidator
{
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

        var fullPath = Path.GetFullPath(folderPath);
        var root = Path.GetPathRoot(fullPath) ?? string.Empty;

        if (root.StartsWith("C:", StringComparison.OrdinalIgnoreCase))
        {
            return new OperationResult(false, "为了避免影响系统盘和系统目录，本工具不支持修改 C 盘文件夹图标。");
        }

        if (Uri.TryCreate(fullPath, UriKind.Absolute, out var uri) && uri.IsUnc)
        {
            return new OperationResult(false, "不支持网络目录。");
        }

        if (!Directory.Exists(fullPath))
        {
            return new OperationResult(false, "目标文件夹不存在。");
        }

        return new OperationResult(true, "目标文件夹有效。");
    }
}
