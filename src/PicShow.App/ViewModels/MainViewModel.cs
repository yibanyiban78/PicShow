using PicShow.Infrastructure;
using PicShow.Models;
using PicShow.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace PicShow.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly MediaDetectionService mediaDetectionService = new();
    private readonly FileDiscoveryService fileDiscoveryService;
    private MediaFile? selectedFile;
    private TransparencyBackground transparencyBackground = TransparencyBackground.Checkerboard;
    private string statusText = "Ready";

    public MainViewModel()
    {
        fileDiscoveryService = new FileDiscoveryService(mediaDetectionService);
        OpenFileCommand = new RelayCommand(OpenFileFromDialog);
        PreviousCommand = new RelayCommand(Previous);
        NextCommand = new RelayCommand(Next);
    }

    public ObservableCollection<MediaFile> Files { get; } = [];

    public ICommand OpenFileCommand { get; }

    public ICommand PreviousCommand { get; }

    public ICommand NextCommand { get; }

    public MediaFile? SelectedFile
    {
        get => selectedFile;
        set
        {
            if (SetProperty(ref selectedFile, value))
            {
                StatusText = value is null ? "Ready" : $"Viewing {value.DisplayName}";
                OnPropertyChanged(nameof(CurrentFileSummary));
            }
        }
    }

    public TransparencyBackground TransparencyBackground
    {
        get => transparencyBackground;
        set => SetProperty(ref transparencyBackground, value);
    }

    public string StatusText
    {
        get => statusText;
        set => SetProperty(ref statusText, value);
    }

    public string CurrentFileSummary => SelectedFile is null
        ? "No file"
        : $"{SelectedFile.DisplayName} ({SelectedFile.Kind})";

    public void OpenFileFromDialog()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "打开图片或 PDF",
            Filter = BuildOpenFileFilter()
        };

        if (dialog.ShowDialog() == true)
        {
            LoadFile(dialog.FileName);
        }
    }

    public void LoadFile(string path)
    {
        var file = fileDiscoveryService.CreateFile(path);
        if (file is null)
        {
            StatusText = "Unsupported file";
            return;
        }

        LoadSiblingFiles(file);
        SelectedFile = Files.FirstOrDefault(item => string.Equals(item.Path, file.Path, StringComparison.OrdinalIgnoreCase)) ?? file;
    }

    private void LoadSiblingFiles(MediaFile file)
    {
        Files.Clear();

        var folder = Path.GetDirectoryName(file.Path);
        var files = string.IsNullOrWhiteSpace(folder)
            ? []
            : fileDiscoveryService.DiscoverFolder(folder);

        if (files.Count == 0)
        {
            Files.Add(file);
            return;
        }

        foreach (var item in files)
        {
            Files.Add(item);
        }
    }

    private void Previous()
    {
        MoveSelection(-1);
    }

    private void Next()
    {
        MoveSelection(1);
    }

    private void MoveSelection(int offset)
    {
        if (Files.Count == 0 || SelectedFile is null)
        {
            return;
        }

        var index = Files.IndexOf(SelectedFile);
        var nextIndex = Math.Clamp(index + offset, 0, Files.Count - 1);
        SelectedFile = Files[nextIndex];
    }

    private string BuildOpenFileFilter()
    {
        var extensions = string.Join(";", mediaDetectionService.SupportedExtensions.Select(extension => $"*{extension}"));
        return $"支持的文件|{extensions}|所有文件|*.*";
    }
}
