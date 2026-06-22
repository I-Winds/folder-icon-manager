namespace FolderIconManager.Services;

public interface IDesktopIniEditor
{
    void SetIconResource(string folderPath, string iconPath);

    void RemoveIconResource(string folderPath);
}
