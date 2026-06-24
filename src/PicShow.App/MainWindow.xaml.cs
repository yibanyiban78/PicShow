using PicShow.ViewModels;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WinForms = System.Windows.Forms;

namespace PicShow;

public partial class MainWindow : Window
{
    private const double WelcomeWindowRatio = 0.85;
    private const double PreviewWindowRatio = 0.85;
    private const double ZoomStep = 1.1;
    private const double MinimumWindowImageMaxSide = 100.0;
    private const double WelcomeCornerRadius = 10.0;
    private const int ControlsAutoHideDelayMs = 2000;
    private static readonly Duration ControlsFadeDuration = TimeSpan.FromMilliseconds(100);
    private const int DwmWindowCornerPreference = 33;
    private const int DwmWindowCornerPreferenceDoNotRound = 1;

    private bool initialSizeApplied;
    private bool isPreviewMode;
    private System.Windows.Size? currentBitmapSize;
    private readonly DispatcherTimer controlsAutoHideTimer;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
        PreviewSurface.BitmapLoaded += PreviewSurface_BitmapLoaded;
        PreviewSurface.MouseWheelRequested += PreviewSurface_MouseWheelRequested;
        PreviewSurface.IsImagePanEnabled = false;
        controlsAutoHideTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ControlsAutoHideDelayMs) };
        controlsAutoHideTimer.Tick += ControlsAutoHideTimer_Tick;
    }

    private MainViewModel ViewModel => (MainViewModel)DataContext;

    public void LoadStartupFile(string path)
    {
        ViewModel.LoadFile(path);
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateRoundRectRgn(int left, int top, int right, int bottom, int width, int height);

    [DllImport("user32.dll")]
    private static extern int SetWindowRgn(IntPtr hwnd, IntPtr region, bool redraw);

    private void Window_SourceInitialized(object? sender, EventArgs e)
    {
        ApplyWelcomeRoundedCorners();
    }

    private void DisableRoundedCorners()
    {
        try
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            _ = SetWindowRgn(hwnd, IntPtr.Zero, true);
            var preference = DwmWindowCornerPreferenceDoNotRound;
            _ = DwmSetWindowAttribute(hwnd, DwmWindowCornerPreference, ref preference, sizeof(int));
        }
        catch
        {
            // Older Windows versions simply ignore this visual refinement.
        }
    }

    private void ApplyWelcomeRoundedCorners()
    {
        if (isPreviewMode)
        {
            return;
        }

        try
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd == IntPtr.Zero || ActualWidth <= 0 || ActualHeight <= 0)
            {
                return;
            }

            var source = PresentationSource.FromVisual(this);
            var scaleX = source?.CompositionTarget?.TransformToDevice.M11 ?? 1.0;
            var scaleY = source?.CompositionTarget?.TransformToDevice.M22 ?? 1.0;
            var width = Math.Max(1, (int)Math.Round(ActualWidth * scaleX));
            var height = Math.Max(1, (int)Math.Round(ActualHeight * scaleY));
            var cornerWidth = Math.Max(1, (int)Math.Round(WelcomeCornerRadius * 2 * scaleX));
            var cornerHeight = Math.Max(1, (int)Math.Round(WelcomeCornerRadius * 2 * scaleY));
            var region = CreateRoundRectRgn(0, 0, width + 1, height + 1, cornerWidth, cornerHeight);

            if (region != IntPtr.Zero)
            {
                _ = SetWindowRgn(hwnd, region, true);
            }
        }
        catch
        {
            // Region clipping is a cosmetic welcome-page detail.
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        ApplyWelcomeWindowBounds();
    }

    private void ApplyWelcomeWindowBounds()
    {
        if (initialSizeApplied)
        {
            return;
        }

        initialSizeApplied = true;

        var bounds = GetCurrentScreenBounds(useFullBounds: false);
        Width = Math.Max(MinWidth, bounds.Width * WelcomeWindowRatio);
        Height = Math.Max(MinHeight, bounds.Height * WelcomeWindowRatio);
        CenterInside(bounds);
        ApplyWelcomeRoundedCorners();
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyWelcomeRoundedCorners();
    }

    private void PreviewSurface_BitmapLoaded(object? sender, System.Windows.Size bitmapSize)
    {
        isPreviewMode = true;
        DisableRoundedCorners();
        currentBitmapSize = bitmapSize;
        ShowWindowControls();
        RestartControlsAutoHideTimer();
        ApplyPreviewWindowScale(GetInitialPreviewScale(bitmapSize), useFullBounds: false);
    }

    private void PreviewSurface_MouseWheelRequested(object? sender, int delta)
    {
        if (currentBitmapSize is null)
        {
            return;
        }

        ShowWindowControls();
        RestartControlsAutoHideTimer();

        var bitmapSize = currentBitmapSize.Value;
        var screenBounds = GetCurrentScreenBounds(useFullBounds: true);
        var currentScale = GetWindowImageScale(bitmapSize);
        var maxWindowScale = GetMaxWindowScale(bitmapSize, screenBounds);
        var minWindowScale = GetMinWindowScale(bitmapSize);
        var isScreenFilled = IsScreenFilled(screenBounds);

        if (delta > 0)
        {
            if (!isScreenFilled)
            {
                var nextScale = currentScale * ZoomStep;
                if (nextScale >= maxWindowScale - 0.001)
                {
                    ApplyScreenFilledWindowBounds(screenBounds);
                }
                else
                {
                    ApplyPreviewWindowScale(nextScale, useFullBounds: true);
                }
            }
            else
            {
                PreviewSurface.ZoomIn();
            }
        }
        else
        {
            if (isScreenFilled && !PreviewSurface.IsAtFitScale)
            {
                PreviewSurface.ZoomOut();
            }
            else
            {
                var scaleFrom = isScreenFilled ? maxWindowScale : currentScale;
                ApplyPreviewWindowScale(Math.Max(scaleFrom / ZoomStep, minWindowScale), useFullBounds: true);
            }
        }
    }

    private double GetInitialPreviewScale(System.Windows.Size bitmapSize)
    {
        var bounds = GetCurrentScreenBounds(useFullBounds: false);
        return Math.Min(bounds.Width * PreviewWindowRatio / bitmapSize.Width, bounds.Height * PreviewWindowRatio / bitmapSize.Height);
    }

    private static double GetMinWindowScale(System.Windows.Size bitmapSize)
    {
        var maxSide = Math.Max(bitmapSize.Width, bitmapSize.Height);
        return maxSide <= 0 ? 1 : MinimumWindowImageMaxSide / maxSide;
    }

    private static double GetMaxWindowScale(System.Windows.Size bitmapSize, Rect bounds)
    {
        return Math.Min(bounds.Width / bitmapSize.Width, bounds.Height / bitmapSize.Height);
    }

    private double GetWindowImageScale(System.Windows.Size bitmapSize)
    {
        return Math.Min(Width / bitmapSize.Width, Height / bitmapSize.Height);
    }

    private bool IsScreenFilled(Rect bounds)
    {
        return Math.Abs(Left - bounds.Left) < 1
            && Math.Abs(Top - bounds.Top) < 1
            && Math.Abs(Width - bounds.Width) < 1
            && Math.Abs(Height - bounds.Height) < 1;
    }

    private void ApplyScreenFilledWindowBounds(Rect bounds)
    {
        PreviewSurface.FitToViewport(bounds.Width, bounds.Height);
        Left = bounds.Left;
        Top = bounds.Top;
        Width = bounds.Width;
        Height = bounds.Height;
        PreviewSurface.IsImagePanEnabled = true;
    }

    private void ApplyPreviewWindowScale(double scale, bool useFullBounds)
    {
        if (currentBitmapSize is null)
        {
            return;
        }

        var bitmapSize = currentBitmapSize.Value;
        var bounds = GetCurrentScreenBounds(useFullBounds);
        var maxScale = GetMaxWindowScale(bitmapSize, bounds);
        var minScale = GetMinWindowScale(bitmapSize);
        var clampedScale = Math.Clamp(scale, minScale, maxScale);

        PreviewSurface.IsImagePanEnabled = false;
        var targetWidth = bitmapSize.Width * clampedScale;
        var targetHeight = bitmapSize.Height * clampedScale;
        var targetLeft = bounds.Left + (bounds.Width - targetWidth) / 2;
        var targetTop = bounds.Top + (bounds.Height - targetHeight) / 2;

        PreviewSurface.FitToViewport(targetWidth, targetHeight);
        Left = targetLeft;
        Top = targetTop;
        Width = targetWidth;
        Height = targetHeight;
    }

    private Rect GetCurrentScreenBounds(bool useFullBounds)
    {
        var source = PresentationSource.FromVisual(this);
        var transform = source?.CompositionTarget?.TransformFromDevice ?? Matrix.Identity;
        var screen = WinForms.Screen.FromHandle(new WindowInteropHelper(this).Handle);
        var area = useFullBounds ? screen.Bounds : screen.WorkingArea;
        var topLeft = transform.Transform(new System.Windows.Point(area.Left, area.Top));
        var bottomRight = transform.Transform(new System.Windows.Point(area.Right, area.Bottom));
        return new Rect(topLeft, bottomRight);
    }

    private void CenterInside(Rect bounds)
    {
        Left = bounds.Left + (bounds.Width - Width) / 2;
        Top = bounds.Top + (bounds.Height - Height) / 2;
    }

    private void Window_Drop(object sender, System.Windows.DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
        {
            return;
        }

        var paths = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
        if (paths.Length > 0)
        {
            ViewModel.LoadFile(paths[0]);
        }
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.O)
        {
            ViewModel.OpenFileFromDialog();
            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            ViewModel.PreviousCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            ViewModel.NextCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
        }
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed && e.ClickCount == 1 && !PreviewSurface.IsImagePanEnabled)
        {
            DragMove();
        }
    }

    private void WindowControls_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (currentBitmapSize is not null)
        {
            ShowWindowControls();
            RestartControlsAutoHideTimer();
        }
    }

    private void WindowControls_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (currentBitmapSize is not null)
        {
            ShowWindowControls();
            RestartControlsAutoHideTimer();
        }
    }

    private void ImageEditToolbar_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (currentBitmapSize is not null)
        {
            ShowWindowControls();
            RestartControlsAutoHideTimer();
        }
    }

    private void ImageEditToolbar_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (currentBitmapSize is not null)
        {
            ShowWindowControls();
            RestartControlsAutoHideTimer();
        }
    }

    private void ActualSizeButton_Click(object sender, RoutedEventArgs e)
    {
        PreviewSurface.ActualSize();
    }

    private void RotateCounterClockwiseButton_Click(object sender, RoutedEventArgs e)
    {
        PreviewSurface.RotateCounterClockwise();
        ShowWindowControls();
        RestartControlsAutoHideTimer();
    }

    private void RotateClockwiseButton_Click(object sender, RoutedEventArgs e)
    {
        PreviewSurface.RotateClockwise();
        ShowWindowControls();
        RestartControlsAutoHideTimer();
    }

    private void FlipHorizontalButton_Click(object sender, RoutedEventArgs e)
    {
        PreviewSurface.FlipHorizontal();
        ShowWindowControls();
        RestartControlsAutoHideTimer();
    }

    private void FlipVerticalButton_Click(object sender, RoutedEventArgs e)
    {
        PreviewSurface.FlipVertical();
        ShowWindowControls();
        RestartControlsAutoHideTimer();
    }

    private void SaveImageButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            PreviewSurface.SaveCurrentImageEdits();
        }
        catch (Exception exception)
        {
            System.Windows.MessageBox.Show(this, exception.Message, "PicShow", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        ShowWindowControls();
        RestartControlsAutoHideTimer();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ControlsAutoHideTimer_Tick(object? sender, EventArgs e)
    {
        controlsAutoHideTimer.Stop();
        HideWindowControls();
    }

    private void RestartControlsAutoHideTimer()
    {
        controlsAutoHideTimer.Stop();
        controlsAutoHideTimer.Start();
    }

    private void ShowWindowControls()
    {
        FadeWindowControlsTo(1);
        FadeImageEditToolbarTo(1);
    }

    private void HideWindowControls()
    {
        FadeWindowControlsTo(0);
        FadeImageEditToolbarTo(0);
    }

    private void FadeWindowControlsTo(double opacity)
    {
        var animation = new DoubleAnimation
        {
            To = opacity,
            Duration = ControlsFadeDuration,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            FillBehavior = FillBehavior.HoldEnd
        };

        WindowControls.BeginAnimation(OpacityProperty, animation, HandoffBehavior.SnapshotAndReplace);
    }

    private void FadeImageEditToolbarTo(double opacity)
    {
        var animation = new DoubleAnimation
        {
            To = opacity,
            Duration = ControlsFadeDuration,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            FillBehavior = FillBehavior.HoldEnd
        };

        ImageEditToolbar.BeginAnimation(OpacityProperty, animation, HandoffBehavior.SnapshotAndReplace);
    }
}
