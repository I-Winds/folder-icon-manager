using System.Runtime.InteropServices;

namespace FolderIconManager.Interop;

internal static class ShellInterop
{
    internal const uint ShcneUpdatedir = 0x00001000;
    internal const uint ShcneUpdateitem = 0x00002000;
    internal const uint ShcnfPathW = 0x0005;

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    internal static extern void SHChangeNotify(uint eventId, uint flags, string item1, IntPtr item2);
}
