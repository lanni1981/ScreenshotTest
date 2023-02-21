using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Cursor = System.Windows.Input.Cursor;

namespace ScreenShot
{
    internal class BitmapCursor : SafeHandle
    {
        public override bool IsInvalid
        {
            get
            {
                return handle.Equals(new IntPtr(-1));
            }
        }

        public static Cursor CreateBmpCursor(Bitmap cursorBitmap)
        {
            return CursorInteropHelper.Create(new BitmapCursor(cursorBitmap));
        }

        protected BitmapCursor(Bitmap cursorBitmap) : base(new IntPtr(-1), true)
        {
            handle = cursorBitmap.GetHicon();
        }

        protected override bool ReleaseHandle()
        {
            bool result = DestroyIcon(handle);
            handle = new IntPtr(-1);
            return result;
        }

        [DllImport("user32.dll")]
        public static extern bool DestroyIcon(IntPtr hIcon);


        public static Cursor CreateCrossCursor()
        {
            const int w = 150;
            const int h = 150;
            using (var bmp = new Bitmap(w, h))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.Default;
                    g.InterpolationMode = InterpolationMode.High;
                    using (var P = new System.Drawing.Pen(System.Drawing.Brushes.Red, 1))
                    {
                        g.DrawLine(P, new System.Drawing.Point(w / 2, 0), new System.Drawing.Point(w / 2, System.Convert.ToInt32(w * 0.45)));
                        g.DrawLine(P, new System.Drawing.Point(w / 2, System.Convert.ToInt32(w * 0.55)), new System.Drawing.Point(w / 2, w));
                        g.DrawLine(P, new System.Drawing.Point(0, w / 2), new System.Drawing.Point(System.Convert.ToInt32(w * 0.45), w / 2));
                        g.DrawLine(P, new System.Drawing.Point(System.Convert.ToInt32(w * 0.55), w / 2), new System.Drawing.Point(w, w / 2));
                        g.DrawLine(P, new System.Drawing.Point(w / 2, w / 2), new System.Drawing.Point(w / 2, w / 2 + 1));
                        g.Flush();
                    }
                }
                var c = CreateBmpCursor(bmp);
                return c;
            }
        }
    }
}
