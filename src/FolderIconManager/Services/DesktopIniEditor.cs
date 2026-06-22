using System.Runtime.InteropServices;
using FolderIconManager.Interop;

namespace FolderIconManager.Services;

public sealed class DesktopIniEditor : IDesktopIniEditor
{
    public void RemoveIconResource(string folderPath)
    {
        WriteIconResource(folderPath, string.Empty);
    }

    public void SetIconResource(string folderPath, string iconPath)
    {
        WriteIconResource(folderPath, iconPath);
    }

    private static void WriteIconResource(string folderPath, string iconPath)
    {
        var settings = new ShellFolderCustomSettings
        {
            Size = (uint)Marshal.SizeOf<ShellFolderCustomSettings>(),
            Mask = ShellInterop.FolderCustomSettingsIconFile,
            IconFile = iconPath,
            IconIndex = 0
        };

        var hResult = ShellInterop.SHGetSetFolderCustomSettings(
            ref settings,
            folderPath,
            ShellInterop.FolderCustomSettingsForceWrite);
        Marshal.ThrowExceptionForHR(hResult);
    }
}
