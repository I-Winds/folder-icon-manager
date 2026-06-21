using System.IO;
using System.Text.Json;

namespace FolderIconManager.Services;

public sealed class FavoriteStore
{
    private const string FileName = "favorites.json";
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;

    public FavoriteStore()
    {
        _filePath = Path.Combine(AppContext.BaseDirectory, FileName);
    }

    public IReadOnlyList<string> Load()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        var json = File.ReadAllText(_filePath);
        var data = JsonSerializer.Deserialize<FavoriteStoreData>(json, SerializerOptions);
        return data?.Folders ?? [];
    }

    public void Save(IEnumerable<string> folderPaths)
    {
        var data = new FavoriteStoreData
        {
            Folders = folderPaths.ToList()
        };
        var json = JsonSerializer.Serialize(data, SerializerOptions);
        File.WriteAllText(_filePath, json);
    }

    private sealed class FavoriteStoreData
    {
        public List<string> Folders { get; set; } = [];
    }
}
