using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace ScreenShot
{
    internal class MaskCanvas : Canvas
    {
        private enum UpdateMaskType
        {
            ForMouseMoving,
            ForMouseLeftButtonUp
        }
        private IndicatorObject? indicator;
        private Point? selectionStartPoint;
        private Point? selectionEndPoint;
        private Rect selectionRegion = Rect.Empty;
        private bool isMaskDraging;

        private Rectangle selectionBorder = new Rectangle();
        private Rectangle maskRectLeft = new Rectangle();
        private Rectangle maskRectRight = new Rectangle();
        private Rectangle maskRectTop = new Rectangle();
        private Rectangle maskRectBottom = new Rectangle();
        public MaskWindow MaskWindowOwner { get; set; }
        public Size? DefaultSize { get; set; }

        public MaskCanvas()
        {
            Loaded += (s, e) =>
                {
                    // make the render effect same as SnapsToDevicePixels
                    // "SnapsToDevicePixels = true;" doesn't work on "OnRender"
                    // however, this maybe make some offset form the render target's origin location
                    // SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

                    // ini this
                    Cursor = MaskWindowOwner.cursor;
                    Background = Brushes.Transparent;

                    // ini maskWin rect
                    maskRectBottom.Fill = ScreenShotConfig.MaskWindowBackground;
                    maskRectTop.Fill = maskRectBottom.Fill;
                    maskRectRight.Fill = maskRectTop.Fill;
                    maskRectLeft.Fill = maskRectRight.Fill;

                    // these propeties(x, y...) will not changed
                    SetLeft(maskRectLeft, 0);
                    SetTop(maskRectLeft, 0);
                    SetRight(maskRectRight, 0);
                    SetTop(maskRectRight, 0);
                    SetTop(maskRectTop, 0);
                    SetBottom(maskRectBottom, 0);
                    maskRectLeft.Height = ActualHeight;


                    Children.Add(maskRectLeft);
                    Children.Add(maskRectRight);
                    Children.Add(maskRectTop);
                    Children.Add(maskRectBottom);

                    // ini selection border
                    selectionBorder.Stroke = ScreenShotConfig.SelectionBorderBrush;
                    selectionBorder.StrokeThickness = ScreenShotConfig.SelectionBorderThickness.Left;
                    Children.Add(selectionBorder);

                    // ini indicator
                    indicator = new IndicatorObject(this);
                    Children.Add(indicator);
                    indicator.Visibility = Visibility.Collapsed;

                    CompositionTarget.Rendering += UpdateLayout;
                };
            Unloaded += (s, e) =>
                            {
                                indicator = null;
                                selectionStartPoint = null;
                                selectionEndPoint = null;
                                this.DataContext = null;
                                this.Resources.Clear();
                                GC.Collect();
                            };
        }
        private void UpdateLayout(object? s, EventArgs e)
        {
            UpdateSelectionBorderLayout();
            UpdateMaskRectanglesLayout();
        }
        private void UpdateSelectionBorderLayout()
        {
            if (!selectionRegion.IsEmpty)
            {
                SetLeft(selectionBorder, selectionRegion.Left);
                SetTop(selectionBorder, selectionRegion.Top);
                selectionBorder.Width = selectionRegion.Width;
                selectionBorder.Height = selectionRegion.Height;
            }
        }
        private void UpdateMaskRectanglesLayout()
        {
            var _ActualHeight = ActualHeight;
            var _ActualWidth = ActualWidth;

            if (selectionRegion.IsEmpty)
            {
                SetLeft(maskRectLeft, 0);
                SetTop(maskRectLeft, 0);
                maskRectLeft.Width = _ActualWidth;
                maskRectLeft.Height = _ActualHeight;

                maskRectBottom.Height = 0;
                maskRectBottom.Width = maskRectBottom.Height;
                maskRectTop.Height = maskRectBottom.Width;
                maskRectTop.Width = maskRectTop.Height;
                maskRectRight.Height = maskRectTop.Width;
                maskRectRight.Width = maskRectRight.Height;
            }
            else
            {
                var temp = selectionRegion.Left;
                if (maskRectLeft.Width != temp) maskRectLeft.Width = temp < 0 ? 0 : temp; // Math.Max(0, selectionRegion.Left);

                temp = ActualWidth - selectionRegion.Right;
                if (maskRectRight.Width != temp) maskRectRight.Width = temp < 0 ? 0 : temp; // Math.Max(0, ActualWidth - selectionRegion.Right);
                if (maskRectRight.Height != _ActualHeight) maskRectRight.Height = _ActualHeight;

                SetLeft(maskRectTop, maskRectLeft.Width);
                SetLeft(maskRectBottom, maskRectLeft.Width);

                temp = _ActualWidth - maskRectLeft.Width - maskRectRight.Width;
                if (maskRectTop.Width != temp) maskRectTop.Width = temp < 0 ? 0 : temp; // Math.Max(0, ActualWidth - maskRectLeft.Width - maskRectRight.Width);

                temp = selectionRegion.Top;
                if (maskRectTop.Height != temp) maskRectTop.Height = temp < 0 ? 0 : temp; // Math.Max(0, selectionRegion.Top);

                maskRectBottom.Width = maskRectTop.Width;
                temp = _ActualHeight - selectionRegion.Bottom;
                if (maskRectBottom.Height != temp) maskRectBottom.Height = temp < 0 ? 0 : temp; // Math.Max(0, ActualHeight - selectionRegion.Bottom);
            }
        }
        private bool IsMouseOnThis(RoutedEventArgs e)
        {
            return e.Source.Equals(this) || e.Source.Equals(maskRectLeft) || e.Source.Equals(maskRectRight) || e.Source.Equals(maskRectTop) || e.Source.Equals(maskRectBottom);
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            indicator.Visibility = Visibility.Visible;
            // mouse down on this self
            if (IsMouseOnThis(e))
                PrepareShowMask(Mouse.GetPosition(this));
            else if (e.Source.Equals(indicator))
                HandleIndicatorMouseDown(e);
            base.OnMouseLeftButtonDown(e);
        }
        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (IsMouseOnThis(e))
            {
                UpdateSelectionRegion(e, UpdateMaskType.ForMouseMoving);
                e.Handled = true;
            }
            base.OnMouseMove(e);
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (IsMouseOnThis(e))
            {
                UpdateSelectionRegion(e, UpdateMaskType.ForMouseLeftButtonUp);
                FinishShowMask();
            }
            base.OnMouseLeftButtonUp(e);
        }
        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            indicator.Visibility = Visibility.Collapsed;
            selectionRegion = Rect.Empty;
            selectionBorder.Height = 0;
            selectionBorder.Width = selectionBorder.Height;
            ClearSelectionData();
            UpdateMaskRectanglesLayout();
            base.OnMouseRightButtonUp(e);
        }
        internal void HandleIndicatorMouseDown(MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                if (MaskWindowOwner != null)
                {
                    MaskWindowOwner.ClipSnapshot(GetIndicatorRegion());
                    ClearSelectionData();
                }
            }
        }
        private void PrepareShowMask(Point mouseLoc)
        {
            indicator.Visibility = Visibility.Collapsed;
            selectionBorder.Visibility = Visibility.Visible;
            selectionStartPoint = new Point?(mouseLoc);
            if (!IsMouseCaptured) CaptureMouse();
        }
        private void UpdateSelectionRegion(MouseEventArgs e, UpdateMaskType updateType)
        {
            if (updateType == UpdateMaskType.ForMouseMoving && e.LeftButton != MouseButtonState.Pressed)
                selectionStartPoint = null;

            if (selectionStartPoint.HasValue)
            {
                selectionEndPoint = e.GetPosition(this);

                var startPoint = (Point)selectionEndPoint;
                var endPoint = (Point)selectionStartPoint;
                var sX = startPoint.X;
                var sY = startPoint.Y;
                var eX = endPoint.X;
                var eY = endPoint.Y;

                var deltaX = eX - sX;
                var deltaY = eY - sY;

                if (Math.Abs(deltaX) >= SystemParameters.MinimumHorizontalDragDistance || Math.Abs(deltaX) >= SystemParameters.MinimumVerticalDragDistance)
                {
                    isMaskDraging = true;
                    double x = sX < eX ? sX : eX; // Math.Min(sX, eX);
                    double y = sY < eY ? sY : eY; // Math.Min(sY, eY);
                    double w = deltaX < 0 ? -deltaX : deltaX; // Math.Abs(deltaX);
                    double h = deltaY < 0 ? -deltaY : deltaY; // Math.Abs(deltaY);

                    selectionRegion = new Rect(x, y, w, h);
                }
                else if (DefaultSize.HasValue && !DefaultSize.Value.IsEmpty && updateType == UpdateMaskType.ForMouseLeftButtonUp)
                {
                    isMaskDraging = true;
                    selectionRegion = new Rect(startPoint.X, startPoint.Y, DefaultSize.Value.Width, DefaultSize.Value.Height);
                }
                else
                    isMaskDraging = false;
            }
        }
        internal void UpdateSelectionRegion(Rect region)
        {
            selectionRegion = region;
        }
        private void FinishShowMask()
        {
            if (IsMouseCaptured) ReleaseMouseCapture();
            if (isMaskDraging)
            {
                if (MaskWindowOwner != null) MaskWindow.OnShowMaskFinished(selectionRegion);
                UpdateIndicator(selectionRegion);
                ClearSelectionData();
            }
        }
        private void ClearSelectionData()
        {
            isMaskDraging = false;
            selectionBorder.Visibility = Visibility.Collapsed;
            selectionStartPoint = null;
            selectionEndPoint = null;
        }
        private void UpdateIndicator(Rect region)
        {
            if (region.Width < indicator?.MinWidth || region.Height < indicator?.MinHeight) return;
            indicator.Width = region.Width;
            indicator.Height = region.Height;
            SetLeft(indicator, region.Left);
            SetTop(indicator, region.Top);
            indicator.Visibility = Visibility.Visible;
        }
        private Rect GetIndicatorRegion()
        {
            return new Rect(GetLeft(indicator), GetTop(indicator), indicator.ActualWidth, indicator.ActualHeight);
        }

    }
}
