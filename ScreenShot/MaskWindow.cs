using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Timer = System.Windows.Forms.Timer;

namespace ScreenShot
{
    internal class MaskWindow : Window
    {
        private MaskCanvas innerCanvas;
        private Timer timeOutTimmer;
        private readonly ScreenShot _owner;
        public readonly System.Windows.Input.Cursor cursor = BitmapCursor.CreateCrossCursor();

        public MaskWindow(ScreenShot screenCaputreOwner)
        {
            this._owner = screenCaputreOwner;
            this.Loaded += (s, e) =>
            {
                ResourceDictionary dict = new ResourceDictionary();
                dict.Source = new Uri("/ScreenShot;component/Themes/ScreenShot.xaml", UriKind.Relative);
                this.Resources.MergedDictionaries.Add(dict);
            };
            this.Closing += (s, e) =>
            {
                cursor.Dispose();
                Background = null;
                this.DataContext = null;
                this.Content = null;
                this.Resources.Clear();
                this.UpdateLayout();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                base.OnClosed(e);
            };
            Ini();
        }

        private void Ini()
        {
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            //Topmost = true;

            var rect = SystemInformation.VirtualScreen; //设置边界以覆盖所有屏幕
            Left = rect.X;
            Top = rect.Y;
            Width = rect.Width;
            Height = rect.Height;

            // set background 
            using var screenSnapshot = ScreenShotExtension.GetScreenSnapshot();
            if (screenSnapshot != null) Background = screenSnapshot.ExToImageBrush();

            // ini canvas
            innerCanvas = new MaskCanvas() { MaskWindowOwner = this };
            Content = innerCanvas;
            this.Activate();
            innerCanvas.Focus();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.RightButton == MouseButtonState.Pressed && e.ClickCount >= 2) CancelCaputre();
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (timeOutTimmer != null && timeOutTimmer.Enabled)
            {
                timeOutTimmer.Stop();
                timeOutTimmer.Start();
            }
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape) CancelCaputre();
        }

        private void CancelCaputre()
        {
            _owner.OnScreenCaputreCancelled(null);
            Close();
        }

        internal static void OnShowMaskFinished(Rect maskRegion) { }

        internal void ClipSnapshot(Rect clipRegion)
        {
            BitmapSource? caputredBmp = CopyFromScreenSnapshot(clipRegion);
            this.UpdateLayout();
            if (caputredBmp != null) _owner.OnScreenCaputred(null, caputredBmp);
            Close();
        }

        internal BitmapSource? CopyFromScreenSnapshot(Rect region)
        {
            innerCanvas.Visibility = Visibility.Hidden;
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)region.Width, (int)region.Height, 96, 96, PixelFormats.Pbgra32);
            var brush = new VisualBrush(this) { Viewbox = region, ViewboxUnits = BrushMappingMode.Absolute };
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                drawingContext.DrawRectangle(brush, null, new Rect(new Point(), region.Size));
            rtb.Render(drawingVisual); // 将绘制内容渲染到BitmapSource中
            BitmapSource result = rtb.Clone();
            rtb.Clear();
            brush = null;
            drawingVisual = null;
            rtb = null;
            innerCanvas.Visibility = Visibility.Visible;
            return result;
        }

        public void Show(int timeOutSecond, System.Windows.Size defaultSize)
        {
            if (timeOutSecond > 0)
            {
                if (timeOutTimmer == null)
                {
                    timeOutTimmer = new Timer();
                    timeOutTimmer.Tick += OnTimeOutTimmerTick;
                }
                timeOutTimmer.Interval = timeOutSecond * 1000;
                timeOutTimmer.Start();
            }
            if (innerCanvas != null) innerCanvas.DefaultSize = defaultSize;
            Show();
            Focus();
        }

        private void OnTimeOutTimmerTick(object? sender, System.EventArgs e)
        {
            timeOutTimmer?.Stop();
            CancelCaputre();
        }
    }

}
