using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScreenShot
{
    internal static class ScreenShotExtension
    {
        public static Rect ToRect(this Rectangle rectangle)
        {
            return new Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        public static Rectangle ToRectangle(this Rect rect) => new Rectangle((int)(rect.X), (int)(rect.Y), (int)(rect.Width), (int)(rect.Height));

        public static Rect GetRectContainsAllScreens()
        {
            var rect = Rect.Empty;
            foreach (Screen screen in Screen.AllScreens)
                rect.Union(screen.Bounds.ToRect());
            return rect;
        }

        public static Bitmap GetScreenSnapshot()
        {
            try
            {
                Rectangle rc = SystemInformation.VirtualScreen;
                using (var bitmap = new Bitmap(width: rc.Width, height: rc.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    if (bitmap == null) return null;
                    using (Graphics memoryGrahics = Graphics.FromImage(bitmap))
                    {
                        memoryGrahics.CopyFromScreen(rc.X, rc.Y, 0, 0, rc.Size, CopyPixelOperation.SourceCopy);
                        memoryGrahics.Dispose();
                    }
                    return (Bitmap)bitmap.Clone(); // 返回一个新的Bitmap对象，以确保在返回后使用后及时释放资源
                }
            }
            catch { }
            return null;
        }

        public static T GetAncestor<T>(this DependencyObject element)
        {
            while (!(element == null || element is T)) element = VisualTreeHelper.GetParent(element);
            if ((element != null) && (element is T)) return (T)(object)element;
            return default(T);
        }

        public static T GetRenderTransform<T>(this UIElement element) where T : Transform
        {
            if (element.RenderTransform.Value.IsIdentity) element.RenderTransform = CreateSimpleTransformGroup();
            if (element.RenderTransform is T) return (T)element.RenderTransform;
            if (element.RenderTransform is TransformGroup)
            {
                var group = (TransformGroup)element.RenderTransform;
                foreach (var t in group.Children)
                {
                    if (t is T) return (T)t;
                }
            }
            throw new NotSupportedException($"Can not get instance of {typeof(T).Name} from {element.ToString}'s RenderTransform : {element.RenderTransform.ToString()}");
        }

        public static TransformGroup CreateSimpleTransformGroup()
        {
            var group = new TransformGroup();
            // notes that : the RotateTransform must must be the first one in this group
            group.Children.Add(new RotateTransform());
            group.Children.Add(new TranslateTransform());
            group.Children.Add(new ScaleTransform());
            group.Children.Add(new SkewTransform());
            return group;
        }

        public static bool IsNormalNumber(this double d)
        {
            return (!double.IsInfinity(d)) && (!double.IsNaN(d)) && (!double.IsNegativeInfinity(d)) && !double.IsPositiveInfinity(d);
        }

        /// <summary>
        /// Bitmap转ImageBrush
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static ImageBrush ExToImageBrush(this Bitmap bmp)
        {
            IntPtr hBitmap = bmp.GetHbitmap();
            var bs = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = bs;
            brush.Freeze();
            DeleteObject(hBitmap);
            return brush;
        }
        [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
    }
}
