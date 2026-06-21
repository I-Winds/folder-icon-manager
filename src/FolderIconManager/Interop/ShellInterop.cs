using System.Runtime.InteropServices;

namespace FolderIconManager.Interop;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct ShellFolderCustomSettings
{
    internal uint Size;
    internal uint Mask;
    internal IntPtr ViewId;

    [MarshalAs(UnmanagedType.LPWStr)]
    internal string? WebViewTemplate;

    internal uint WebViewTemplateLength;

    [MarshalAs(UnmanagedType.LPWStr)]
    internal string? WebViewTemplateVersion;

    [MarshalAs(UnmanagedType.LPWStr)]
    internal string? InfoTip;

    internal uint InfoTipLength;
    internal IntPtr ClassId;
    internal uint Flags;

    [MarshalAs(UnmanagedType.LPWStr)]
    internal string? IconFile;

    internal uint IconFileLength;
    internal int IconIndex;

    [MarshalAs(UnmanagedType.LPWStr)]
    internal string? Logo;

    internal uint LogoLength;
}

internal static class ShellInterop
{
    internal const uint FolderCustomSettingsIconFile = 0x00000010;
    internal const uint FolderCustomSettingsForceWrite = 0x00000002;

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    internal static extern int SHGetSetFolderCustomSettings(
        ref ShellFolderCustomSettings settings,
        string folderPath,
        uint readWrite);
}
