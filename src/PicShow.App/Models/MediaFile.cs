using System.IO;

namespace PicShow.Models;

public sealed class MediaFile
{
    public MediaFile(string path, MediaKind kind)
    {
        Path = path;
        Kind = kind;
    }

    public string Path { get; }

    public MediaKind Kind { get; }

    public string DisplayName => System.IO.Path.GetFileName(Path);

    public string Extension => System.IO.Path.GetExtension(Path).ToLowerInvariant();

    public bool Exists => File.Exists(Path);
}
