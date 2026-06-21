using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace FolderIconManager.Models;

public sealed class FavoriteFolderItem
{
    private bool _isExpanded;
    public FavoriteFolderItem(string folderPath)
    {
        FolderPath = folderPath;
        var directoryName = Path.GetFileName(Path.TrimEndingDirectorySeparator(folderPath));
        DisplayName = string.IsNullOrWhiteSpace(directoryName) ? folderPath : directoryName;
    }

    public string FolderPath { get; }

    public string DisplayName { get; }

    public ObservableCollection<FavoriteIconItem> Icons { get; } = [];

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded == value)
            {
                return;
            }

            _isExpanded = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
