using System.IO;
using FolderIconManager.Models;

namespace FolderIconManager.Services;

public sealed class FolderIconService : IFolderIconService
{
    private readonly FolderPathValidator _validator;
    private readonly IDesktopIniEditor _desktopIniEditor;
    private readonly IShellNotifier _shellNotifier;

    public FolderIconService(
        FolderPathValidator validator,
        IDesktopIniEditor desktopIniEditor,
        IShellNotifier shellNotifier)
    {
        _validator = validator;
        _desktopIniEditor = desktopIniEditor;
        _shellNotifier = shellNotifier;
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
            _desktopIniEditor.SetIconResource(folderPath!, iconPath!);
            File.SetAttributes(
                folderPath!,
                File.GetAttributes(folderPath!) | FileAttributes.System);
            _shellNotifier.RefreshFolder(folderPath!);
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
            return new OperationResult(false, "Shell 刷新失败或未能立即生效。");
        }

        return new OperationResult(true, "图标已应用，已通知资源管理器刷新。");
    }

    public OperationResult Restore(string? folderPath)
    {
        var targetValidation = _validator.ValidateTarget(folderPath);
        if (!targetValidation.IsSuccess)
        {
            return targetValidation;
        }

        _desktopIniEditor.RemoveIconResource(folderPath!);
        _shellNotifier.RefreshFolder(folderPath!);

        return new OperationResult(true, "已恢复默认图标，已通知资源管理器刷新。");
    }
}
