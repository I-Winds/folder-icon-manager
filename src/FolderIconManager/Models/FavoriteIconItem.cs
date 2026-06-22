using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FolderIconManager.Models;

public sealed class FavoriteIconItem
{
    private ImageSource? _originalPreview;
    private bool _isOriginalPreviewLoaded;

    public FavoriteIconItem(string iconPath)
    {
        IconPath = iconPath;
        FileName = Path.GetFileName(iconPath);
        ThumbnailPreview = LoadPreview(iconPath, 64);
    }

    public string IconPath { get; }

    public string FileName { get; }

    public ImageSource? ThumbnailPreview { get; }

    public ImageSource? OriginalPreview
    {
        get
        {
            if (!_isOriginalPreviewLoaded)
            {
                _originalPreview = LoadPreview(IconPath, 64);
                _isOriginalPreviewLoaded = true;
            }

            return _originalPreview;
        }
    }

    private static ImageSource? LoadPreview(string iconPath, int? decodePixelWidth)
    {
        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(iconPath, UriKind.Absolute);
            if (decodePixelWidth is not null)
            {
                bitmap.DecodePixelWidth = decodePixelWidth.Value;
            }
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
