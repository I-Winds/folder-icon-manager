using System.IO;
using FolderIconManager.Models;

namespace FolderIconManager.Services;

public sealed class FolderIconService : IFolderIconService
{
    private readonly FolderPathValidator _validator;
    private readonly IDesktopIniEditor _desktopIniEditor;

    public FolderIconService(
        FolderPathValidator validator,
        IDesktopIniEditor desktopIniEditor)
    {
        _validator = validator;
        _desktopIniEditor = desktopIniEditor;
    }

    public OperationResult Apply(string? folderPath, string? iconPath)
    {
        var targetValidation = _validator.ValidateTarget(folderPath);
        if (!targetValidation.IsSuccess)
        {
            return targetValidation;
        }

        var iconValidation = _validator.ValidateIcon(iconPath);
        if (!iconValidation.IsSuccess)
        {
            return iconValidation;
        }

        try
        {
            File.SetAttributes(
                folderPath!,
                File.GetAttributes(folderPath!) | FileAttributes.System);
            _desktopIniEditor.SetIconResource(folderPath!, iconPath!);
        }
        catch (UnauthorizedAccessException)
        {
            return new OperationResult(false, "没有写入权限。");
        }
        catch (IOException)
        {
            return new OperationResult(false, "写入 desktop.ini 失败。");
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
            return new OperationResult(false, "Shell 设置文件夹图标失败。" );
        }

        return new OperationResult(true, "图标已应用。");
    }

    public OperationResult Restore(string? folderPath)
    {
        var targetValidation = _validator.ValidateTarget(folderPath);
        if (!targetValidation.IsSuccess)
        {
            return targetValidation;
        }

        try
        {
            _desktopIniEditor.RemoveIconResource(folderPath!);
        }
        catch (UnauthorizedAccessException)
        {
            return new OperationResult(false, "没有写入权限。");
        }
        catch (IOException)
        {
            return new OperationResult(false, "写入 desktop.ini 失败。");
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
            return new OperationResult(false, "Shell 恢复默认图标失败。" );
        }

        return new OperationResult(true, "已恢复默认图标。");
    }
}
