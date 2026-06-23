using PicShow.Models;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PicShow.Views;

public partial class PreviewHostView : System.Windows.Controls.UserControl
{
    private const double ZoomStep = 1.1;
    private const double MinZoom = 0.05;
    private const double MaxZoom = 16.0;
    private const double CoverOverscanPixels = 2.0;

    private BitmapSource? currentBitmap;
    private double zoom = 1.0;
    private double imageLeft;
    private double imageTop;
    private bool isFitMode = true;
    private bool isDraggingImage;
    private System.Windows.Point dragStartPoint;
    private double dragStartLeft;
    private double dragStartTop;

    public static readonly DependencyProperty CurrentFileProperty = DependencyProperty.Register(
        nameof(CurrentFile),
        typeof(MediaFile),
        typeof(PreviewHostView),
        new PropertyMetadata(null, OnCurrentFileChanged));

    public static readonly DependencyProperty TransparencyBackgroundProperty = DependencyProperty.Register(
        nameof(TransparencyBackground),
        typeof(TransparencyBackground),
        typeof(PreviewHostView),
        new PropertyMetadata(TransparencyBackground.Checkerboard));

    public PreviewHostView()
    {
        InitializeComponent();
        ShowDefaultMessage();
    }

    public event EventHandler<System.Windows.Size>? BitmapLoaded;

    public event EventHandler<int>? MouseWheelRequested;

    public MediaFile? CurrentFile
    {
        get => (MediaFile?)GetValue(CurrentFileProperty);
        set => SetValue(CurrentFileProperty, value);
    }

    public TransparencyBackground TransparencyBackground
    {
        get => (TransparencyBackground)GetValue(TransparencyBackgroundProperty);
        set => SetValue(TransparencyBackgroundProperty, value);
    }

    public bool IsPointerOverImage => PreviewImage.IsMouseOver && currentBitmap is not null;

    public bool CanPanCurrentImage => CanPanImage();

    public bool IsImagePanEnabled { get; set; }

    public bool IsAtFitScale => currentBitmap is null || Math.Abs(zoom - GetCoverScale()) < 0.001;

    public void FitToView()
    {
        if (currentBitmap is null)
        {
            return;
        }

        isFitMode = true;
        zoom = GetCoverScale();
        ApplyZoom(centerImage: true);
    }

    public void ActualSize()
    {
        if (currentBitmap is null)
        {
            return;
        }

        isFitMode = false;
        zoom = 1.0;
        ApplyZoom(centerImage: true);
    }

    public void FitToViewport(double viewportWidth, double viewportHeight)
    {
        if (currentBitmap is null || viewportWidth <= 0 || viewportHeight <= 0)
        {
            return;
        }

        isFitMode = true;
        zoom = GetCoverScale(viewportWidth, viewportHeight);
        PreviewImage.Width = Math.Ceiling(GetDisplayedWidth());
        PreviewImage.Height = Math.Ceiling(GetDisplayedHeight());
        imageLeft = ClampAxis((viewportWidth - PreviewImage.Width) / 2, GetDisplayedWidth(), viewportWidth);
        imageTop = ClampAxis((viewportHeight - PreviewImage.Height) / 2, GetDisplayedHeight(), viewportHeight);
        ApplyImagePosition(animate: false);
    }

    public void ZoomIn()
    {
        ZoomBy(ZoomStep);
    }

    public void ZoomOut()
    {
        ZoomBy(1.0 / ZoomStep);
    }

    public void RotateClockwise()
    {
        ApplyBitmapTransform(new RotateTransform(90));
    }

    public void RotateCounterClockwise()
    {
        ApplyBitmapTransform(new RotateTransform(-90));
    }

    public void FlipHorizontal()
    {
        ApplyBitmapTransform(new ScaleTransform(-1, 1));
    }

    public void FlipVertical()
    {
        ApplyBitmapTransform(new ScaleTransform(1, -1));
    }

    public void SaveCurrentImageEdits()
    {
        if (currentBitmap is null || CurrentFile is null)
        {
            return;
        }

        if (CurrentFile.Kind is MediaKind.AnimatedGif or MediaKind.Svg or MediaKind.Psd or MediaKind.Pdf)
        {
            throw new NotSupportedException("当前格式暂不支持直接保存修改。");
        }

        var encoder = CreateEncoder(CurrentFile.Path);
        encoder.Frames.Add(BitmapFrame.Create(currentBitmap));

        var folder = Path.GetDirectoryName(CurrentFile.Path);
        var tempPath = Path.Combine(string.IsNullOrWhiteSpace(folder) ? Path.GetTempPath() : folder, $".{Path.GetFileName(CurrentFile.Path)}.picshow.tmp");

        using (var stream = File.Open(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            encoder.Save(stream);
        }

        File.Copy(tempPath, CurrentFile.Path, overwrite: true);
        File.Delete(tempPath);
    }

    private static void OnCurrentFileChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        ((PreviewHostView)dependencyObject).LoadCurrentFile();
    }

    private void ApplyBitmapTransform(Transform transform)
    {
        if (currentBitmap is null)
        {
            return;
        }

        var transformed = new TransformedBitmap(currentBitmap, transform);
        transformed.Freeze();
        currentBitmap = transformed;
        PreviewImage.Source = transformed;
        BitmapLoaded?.Invoke(this, new System.Windows.Size(transformed.Width, transformed.Height));
        FitToView();
    }

    private static BitmapEncoder CreateEncoder(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => new JpegBitmapEncoder { QualityLevel = 95 },
            ".png" => new PngBitmapEncoder(),
            ".bmp" => new BmpBitmapEncoder(),
            ".tif" or ".tiff" => new TiffBitmapEncoder(),
            _ => throw new NotSupportedException("当前格式暂不支持直接保存修改。")
        };
    }

    private void LoadCurrentFile()
    {
        currentBitmap = null;
        PreviewImage.Source = null;

        if (CurrentFile is null)
        {
            ShowDefaultMessage();
            return;
        }

        if (!CurrentFile.Exists)
        {
            ShowMessage(CurrentFile.DisplayName, "文件不存在或已被移动。");
            return;
        }

        if (CurrentFile.Kind is MediaKind.RasterImage or MediaKind.AnimatedGif)
        {
            LoadBitmap(CurrentFile.Path);
            return;
        }

        ShowMessage(CurrentFile.DisplayName, $"{CurrentFile.Kind} 文件已识别，专用预览器将在下一步接入。");
    }

    private void LoadBitmap(string path)
    {
        try
        {
            using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreColorProfile;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();

            currentBitmap = bitmap;
            PreviewImage.Source = bitmap;
            WelcomePanel.Visibility = Visibility.Collapsed;
            MessagePanel.Visibility = Visibility.Collapsed;
            BitmapLoaded?.Invoke(this, new System.Windows.Size(bitmap.Width, bitmap.Height));
            FitToView();
        }
        catch (Exception exception)
        {
            ShowMessage(Path.GetFileName(path), $"无法打开该图片：{exception.Message}");
        }
    }

    private void ZoomBy(double factor)
    {
        if (currentBitmap is null)
        {
            return;
        }

        isFitMode = false;
        var oldWidth = GetDisplayedWidth();
        var oldHeight = GetDisplayedHeight();
        var centerX = ActualWidth / 2;
        var centerY = ActualHeight / 2;
        var relativeX = oldWidth > 0 ? (centerX - imageLeft) / oldWidth : 0.5;
        var relativeY = oldHeight > 0 ? (centerY - imageTop) / oldHeight : 0.5;

        zoom = Math.Clamp(zoom * factor, GetCoverScale(), MaxZoom);
        ApplyZoom(centerImage: false);

        imageLeft = centerX - GetDisplayedWidth() * relativeX;
        imageTop = centerY - GetDisplayedHeight() * relativeY;
        ClampImagePosition();
        ApplyImagePosition(animate: true);
    }

    private void ApplyZoom(bool centerImage)
    {
        if (currentBitmap is null)
        {
            return;
        }

        var targetWidth = Math.Ceiling(GetDisplayedWidth());
        var targetHeight = Math.Ceiling(GetDisplayedHeight());
        PreviewImage.Width = targetWidth;
        PreviewImage.Height = targetHeight;

        if (centerImage)
        {
            imageLeft = (ActualWidth - PreviewImage.Width) / 2;
            imageTop = (ActualHeight - PreviewImage.Height) / 2;
        }

        ClampImagePosition();
        ApplyImagePosition(animate: false);
    }

    private double GetCoverScale()
    {
        if (currentBitmap is null || ActualWidth <= 0 || ActualHeight <= 0)
        {
            return 1.0;
        }

        return GetCoverScale(ActualWidth, ActualHeight);
    }

    private double GetCoverScale(double viewportWidth, double viewportHeight)
    {
        if (currentBitmap is null || viewportWidth <= 0 || viewportHeight <= 0)
        {
            return 1.0;
        }

        var widthScale = (viewportWidth + CoverOverscanPixels) / currentBitmap.Width;
        var heightScale = (viewportHeight + CoverOverscanPixels) / currentBitmap.Height;
        return Math.Clamp(Math.Max(widthScale, heightScale), MinZoom, MaxZoom);
    }

    private double GetDisplayedWidth() => currentBitmap is null ? 0 : currentBitmap.Width * zoom;

    private double GetDisplayedHeight() => currentBitmap is null ? 0 : currentBitmap.Height * zoom;

    private void ClampImagePosition()
    {
        if (currentBitmap is null)
        {
            return;
        }

        imageLeft = ClampAxis(imageLeft, GetDisplayedWidth(), ActualWidth);
        imageTop = ClampAxis(imageTop, GetDisplayedHeight(), ActualHeight);
    }

    private static double ClampAxis(double value, double contentLength, double viewportLength)
    {
        if (contentLength <= viewportLength)
        {
            return Math.Floor((viewportLength - contentLength) / 2);
        }

        return Math.Clamp(value, Math.Floor(viewportLength - contentLength), 0);
    }

    private void ApplyImagePosition(bool animate)
    {
        var left = Math.Floor(imageLeft);
        var top = Math.Floor(imageTop);

        Canvas.SetLeft(PreviewImage, left);
        Canvas.SetTop(PreviewImage, top);
    }

    private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (isFitMode)
        {
            FitToView();
        }
        else
        {
            ClampImagePosition();
            ApplyImagePosition(animate: false);
        }
    }

    private void PreviewImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (currentBitmap is null || !IsImagePanEnabled || !CanPanImage())
        {
            return;
        }

        isDraggingImage = true;
        dragStartPoint = e.GetPosition(this);
        dragStartLeft = imageLeft;
        dragStartTop = imageTop;
        PreviewImage.CaptureMouse();
        e.Handled = true;
    }

    private void PreviewImage_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (!isDraggingImage)
        {
            return;
        }

        var currentPoint = e.GetPosition(this);
        imageLeft = dragStartLeft + currentPoint.X - dragStartPoint.X;
        imageTop = dragStartTop + currentPoint.Y - dragStartPoint.Y;
        ClampImagePosition();
        ApplyImagePosition(animate: false);
        e.Handled = true;
    }

    private void PreviewImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        StopImageDrag();
        e.Handled = true;
    }

    private void PreviewImage_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (isDraggingImage && e.LeftButton != MouseButtonState.Pressed)
        {
            StopImageDrag();
        }
    }

    private void Root_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        MouseWheelRequested?.Invoke(this, e.Delta);
        e.Handled = true;
    }

    private bool CanPanImage()
    {
        if (currentBitmap is null)
        {
            return false;
        }

        return GetDisplayedWidth() > ActualWidth || GetDisplayedHeight() > ActualHeight;
    }

    private void StopImageDrag()
    {
        isDraggingImage = false;
        PreviewImage.ReleaseMouseCapture();
    }

    private void ShowDefaultMessage()
    {
        WelcomePanel.Visibility = Visibility.Visible;
        MessagePanel.Visibility = Visibility.Collapsed;
    }

    private void ShowMessage(string title, string detail)
    {
        WelcomePanel.Visibility = Visibility.Collapsed;
        MessageTitle.Text = title;
        MessageDetail.Text = detail;
        MessagePanel.Visibility = Visibility.Visible;
    }
}
