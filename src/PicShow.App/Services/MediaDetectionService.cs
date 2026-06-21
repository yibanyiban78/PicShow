using PicShow.Models;
using System.IO;

namespace PicShow.Services;

public sealed class MediaDetectionService
{
    private static readonly HashSet<string> RasterExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".bmp",
        ".tif",
        ".tiff",
        ".ico",
        ".webp"
    };

    public IReadOnlyCollection<string> SupportedExtensions { get; } =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".bmp",
        ".tif",
        ".tiff",
        ".ico",
        ".webp",
        ".gif",
        ".svg",
        ".psd",
        ".pdf"
    ];

    public bool IsSupported(string path) => Detect(path) != MediaKind.Unknown;

    public MediaKind Detect(string path)
    {
        var extension = Path.GetExtension(path);

        if (string.Equals(extension, ".gif", StringComparison.OrdinalIgnoreCase))
        {
            return MediaKind.AnimatedGif;
        }

        if (string.Equals(extension, ".svg", StringComparison.OrdinalIgnoreCase))
        {
            return MediaKind.Svg;
        }

        if (string.Equals(extension, ".psd", StringComparison.OrdinalIgnoreCase))
        {
            return MediaKind.Psd;
        }

        if (string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return MediaKind.Pdf;
        }

        return RasterExtensions.Contains(extension) ? MediaKind.RasterImage : MediaKind.Unknown;
    }
}
