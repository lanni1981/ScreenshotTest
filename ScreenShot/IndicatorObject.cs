using System.Windows;
using System.Windows.Controls;

namespace ScreenShot
{
    internal class IndicatorObject : ContentControl
    {
        private MaskCanvas? canvasOwner;

        public IndicatorObject(MaskCanvas canvasOwner) : base()
        {
            this.canvasOwner = canvasOwner;
            Unloaded += (s, e) =>
            {
                canvasOwner = null;
                this.DataContext = null;
                this.Resources = null;
            };
        }

        public IndicatorObject()
        {
            var ownerType = typeof(IndicatorObject);
            FocusVisualStyleProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(null));
            DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
            MinWidthProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(5.0));
            MinHeightProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(5.0));
        }

        public void Resize(Rect r)
        {
            Canvas.SetLeft(this, r.X);
            Canvas.SetTop(this, r.Y);
            Width = r.Width;
            Height = r.Height;
            canvasOwner?.UpdateSelectionRegion(r);
        }

        public void Move(Point offset)
        {
            var x = Canvas.GetLeft(this) + offset.X;
            var y = Canvas.GetTop(this) + offset.Y;
            Canvas.SetLeft(this, x);
            Canvas.SetTop(this, y);
            canvasOwner?.UpdateSelectionRegion(new Rect(x, y, Width, Height));
        }
    }
}
