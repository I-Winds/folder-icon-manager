using FolderIconManager.Models;

namespace FolderIconManager.Services;

public interface IFolderIconService
{
    OperationResult Apply(string? folderPath, string? iconPath);

    OperationResult Restore(string? folderPath);
}
