using System.Windows;
using System.Windows.Media;

namespace ScreenShot
{
    internal sealed class ScreenShotConfig
    {
        public static Thickness SelectionBorderThickness = new Thickness(1.0);
        public static Brush SelectionBorderBrush = new SolidColorBrush(Color.FromArgb(255, 49, 106, 196));
        public static Brush MaskWindowBackground = new SolidColorBrush(Color.FromArgb(80, 200, 200, 200));
    }
}
