namespace FolderIconManager.Tests;

public sealed class TestWorkspace : IDisposable
{
    public string RootPath { get; } = Path.Combine(
        FindSolutionRoot(),
        "inbox",
        "测试用例",
        "自动化临时数据",
        Guid.NewGuid().ToString("N"));

    public TestWorkspace()
    {
        Directory.CreateDirectory(RootPath);
    }

    public string CreateDirectory(string name)
    {
        var path = Path.Combine(RootPath, name);
        Directory.CreateDirectory(path);
        return path;
    }

    public string CreateFile(string name, string content = "")
    {
        var path = Path.Combine(RootPath, name);
        File.WriteAllText(path, content);
        return path;
    }

    public void Dispose()
    {
        if (Directory.Exists(RootPath))
        {
            foreach (var file in Directory.EnumerateFiles(RootPath, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }

            foreach (var directory in Directory.EnumerateDirectories(RootPath, "*", SearchOption.AllDirectories))
            {
                var attributes = File.GetAttributes(directory);
                File.SetAttributes(directory, attributes & ~(FileAttributes.Hidden | FileAttributes.ReadOnly | FileAttributes.System));
            }

            Directory.Delete(RootPath, recursive: true);
        }
    }

    private static string FindSolutionRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "FolderIconManager.sln")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("未找到 FolderIconManager.sln。");
    }
}
