using PicShow.Models;
using System.IO;

namespace PicShow.Services;

public sealed class FileDiscoveryService
{
    private readonly MediaDetectionService mediaDetectionService;

    public FileDiscoveryService(MediaDetectionService mediaDetectionService)
    {
        this.mediaDetectionService = mediaDetectionService;
    }

    public IReadOnlyList<MediaFile> DiscoverFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            return [];
        }

        return Directory.EnumerateFiles(folderPath)
            .Where(mediaDetectionService.IsSupported)
            .OrderBy(path => path, StringComparer.CurrentCultureIgnoreCase)
            .Select(path => new MediaFile(path, mediaDetectionService.Detect(path)))
            .ToList();
    }

    public MediaFile? CreateFile(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var kind = mediaDetectionService.Detect(path);
        return kind == MediaKind.Unknown ? null : new MediaFile(path, kind);
    }
}
