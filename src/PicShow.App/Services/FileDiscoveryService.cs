using PicShow.Models;
using System.IO;
using System.Runtime.InteropServices;

namespace PicShow.Services;

public sealed class FileDiscoveryService
{
    private static readonly IComparer<string> FileNameComparer = new NaturalFileNameComparer();
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
            .OrderBy(path => Path.GetFileName(path) ?? string.Empty, FileNameComparer)
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

    private sealed class NaturalFileNameComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (x is null)
            {
                return -1;
            }

            if (y is null)
            {
                return 1;
            }

            return StrCmpLogicalW(x, y);
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string x, string y);
    }
}
