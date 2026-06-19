using System.IO;

namespace FolderIconManager.Services;

public sealed class DesktopIniEditor : IDesktopIniEditor
{
    public void RemoveIconResource(string folderPath)
    {
        var desktopIniPath = Path.Combine(folderPath, "desktop.ini");
        if (!File.Exists(desktopIniPath))
        {
            return;
        }

        var originalContent = File.ReadAllText(desktopIniPath);
        var lineEnding = originalContent.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var lines = File.ReadAllLines(desktopIniPath).ToList();
        var sectionIndex = FindShellClassInfoSection(lines);
        if (sectionIndex < 0)
        {
            return;
        }

        var sectionEnd = FindSectionEnd(lines, sectionIndex + 1);
        for (var index = sectionEnd - 1; index > sectionIndex; index--)
        {
            if (lines[index].StartsWith("IconResource=", StringComparison.OrdinalIgnoreCase))
            {
                lines.RemoveAt(index);
            }
        }

        WriteDesktopIni(desktopIniPath, string.Join(lineEnding, lines) + lineEnding, markAsFolderConfiguration: false);
    }

    public void SetIconResource(string folderPath, string iconPath)
    {
        var desktopIniPath = Path.Combine(folderPath, "desktop.ini");
        var originalContent = File.Exists(desktopIniPath) ? File.ReadAllText(desktopIniPath) : string.Empty;
        var lineEnding = originalContent.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var lines = originalContent.Length == 0
            ? new List<string>()
            : File.ReadAllLines(desktopIniPath).ToList();
        var sectionIndex = FindShellClassInfoSection(lines);
        var iconResource = $"IconResource={Path.GetFullPath(iconPath)},0";

        if (sectionIndex < 0)
        {
            if (lines.Count > 0 && lines[^1].Length > 0)
            {
                lines.Add(string.Empty);
            }

            lines.Add("[.ShellClassInfo]");
            lines.Add(iconResource);
        }
        else
        {
            var sectionEnd = FindSectionEnd(lines, sectionIndex + 1);
            for (var index = sectionEnd - 1; index > sectionIndex; index--)
            {
                if (lines[index].StartsWith("IconResource=", StringComparison.OrdinalIgnoreCase))
                {
                    lines.RemoveAt(index);
                }
            }

            lines.Insert(sectionIndex + 1, iconResource);
        }

        var updatedContent = string.Join(lineEnding, lines) + lineEnding;
        WriteDesktopIni(desktopIniPath, updatedContent, markAsFolderConfiguration: true);
    }

    private static int FindShellClassInfoSection(IReadOnlyList<string> lines)
    {
        for (var index = 0; index < lines.Count; index++)
        {
            if (string.Equals(lines[index].Trim(), "[.ShellClassInfo]", StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return -1;
    }

    private static int FindSectionEnd(IReadOnlyList<string> lines, int startIndex)
    {
        for (var index = startIndex; index < lines.Count; index++)
        {
            if (lines[index].TrimStart().StartsWith("[", StringComparison.Ordinal))
            {
                return index;
            }
        }

        return lines.Count;
    }

    private static void WriteDesktopIni(string desktopIniPath, string content, bool markAsFolderConfiguration)
    {
        var existed = File.Exists(desktopIniPath);
        var originalAttributes = existed ? File.GetAttributes(desktopIniPath) : FileAttributes.Normal;

        if (existed)
        {
            File.SetAttributes(
                desktopIniPath,
                originalAttributes & ~(FileAttributes.Hidden | FileAttributes.ReadOnly | FileAttributes.System));
        }

        try
        {
            File.WriteAllText(desktopIniPath, content);
        }
        finally
        {
            if (File.Exists(desktopIniPath))
            {
                var finalAttributes = markAsFolderConfiguration
                    ? originalAttributes | FileAttributes.Hidden | FileAttributes.System
                    : originalAttributes;
                File.SetAttributes(desktopIniPath, finalAttributes);
            }
        }
    }
}
