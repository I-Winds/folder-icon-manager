using FolderIconManager.Interop;

namespace FolderIconManager.Services;

public sealed class ShellNotifier : IShellNotifier
{
    public void RefreshFolder(string folderPath)
    {
        ShellInterop.SHChangeNotify(ShellInterop.ShcneUpdatedir, ShellInterop.ShcnfPathW, folderPath, IntPtr.Zero);
        ShellInterop.SHChangeNotify(ShellInterop.ShcneUpdateitem, ShellInterop.ShcnfPathW, folderPath, IntPtr.Zero);
    }
}
