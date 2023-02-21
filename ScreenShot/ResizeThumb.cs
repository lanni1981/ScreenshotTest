using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ScreenShot
{
    public enum ResizeThumbPlacementEnum
    {
        None,
        LeftTop,
        TopCenter,
        RightTop,
        RightCenter,
        RightBottom,
        BottomCenter,
        LeftBottom,
        LeftCenter,
    }
    internal class ResizeThumb : ThumbBase
    {
        private RotateTransform rotateTransform;
        private Point transformOrigin;
        private double angle;

        public ResizeThumbPlacementEnum Placement
        {
            get
            {
                return (ResizeThumbPlacementEnum)GetValue(PlacementProperty);
            }
            set
            {
                SetValue(PlacementProperty, value);
            }
        }

        public static readonly DependencyProperty PlacementProperty =
            DependencyProperty.Register("Placement", typeof(ResizeThumbPlacementEnum), typeof(ResizeThumb),new UIPropertyMetadata(ResizeThumbPlacementEnum.None));


        protected override void OnDragStarted(object sender, DragStartedEventArgs e)
        {
            if (Target != null)
            {
                var canvas = VisualTreeHelper.GetParent(Target) as MaskCanvas;
                if (canvas != null)
                {
                    transformOrigin = Target.RenderTransformOrigin;
                    rotateTransform = Target.GetRenderTransform<RotateTransform>();

                    if (rotateTransform != null)
                        angle = rotateTransform.Angle * Math.PI / (double)180;
                    else
                        angle = 0.0;
                }
            }
        }

        protected override void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (Target != null)
            {
                var delta = new Point(e.HorizontalChange, e.VerticalChange);

                var canvas = Target.Parent as MaskCanvas;
                if (canvas != null)
                {
                    double x = System.Windows.Controls.Canvas.GetLeft(Target);
                    double y = System.Windows.Controls.Canvas.GetTop(Target);
                    double w = Target.ActualWidth;
                    double h = Target.ActualHeight;

                    // adjust delta.ZoomFactor when normal resize 
                    switch (Placement)
                    {
                        case ResizeThumbPlacementEnum.LeftTop:
                        case ResizeThumbPlacementEnum.LeftBottom:
                        case ResizeThumbPlacementEnum.LeftCenter:
                            {
                                if (w - delta.X <= Target.MinWidth && delta.X > 0)
                                    delta.X = w - Target.MinWidth;

                                if (w - delta.X >= Target.MaxWidth && delta.X < 0)
                                    delta.X = w - Target.MaxWidth;
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }

                    // adjust delta.Y when normal resize
                    switch (Placement)
                    {
                        case ResizeThumbPlacementEnum.LeftTop:
                        case ResizeThumbPlacementEnum.RightTop:
                        case ResizeThumbPlacementEnum.TopCenter:
                            {
                                if (h - delta.Y <= Target.MinHeight && delta.Y > 0)
                                    delta.Y = h - Target.MinHeight;

                                if (h - delta.Y >= Target.MaxHeight && delta.Y < 0)
                                    delta.Y = h - Target.MaxHeight;
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }

                    // adjust delta.ZoomFactor when rotated an then resize 
                    switch (Placement)
                    {
                        case ResizeThumbPlacementEnum.RightTop:
                        case ResizeThumbPlacementEnum.RightBottom:
                        case ResizeThumbPlacementEnum.RightCenter:
                            {
                                if (w + delta.X <= Target.MinWidth && delta.X < 0)
                                    delta.X = Target.MinWidth - w;

                                if (w + delta.X >= Target.MaxWidth && delta.X > 0)
                                    delta.X = Target.MaxWidth - w;
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }

                    // adjust delta.Y when rotated an then resize 
                    switch (Placement)
                    {
                        case ResizeThumbPlacementEnum.LeftBottom:
                        case ResizeThumbPlacementEnum.RightBottom:
                        case ResizeThumbPlacementEnum.BottomCenter:
                            {
                                if (h + delta.Y <= Target.MinHeight && delta.Y < 0)
                                    delta.Y = Target.MinHeight - h;

                                if (h + delta.Y >= Target.MaxHeight && delta.Y > 0)
                                    delta.Y = Target.MaxHeight - h;
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }

                    switch (Placement)
                    {
                        case ResizeThumbPlacementEnum.LeftTop:
                            {
                                x += delta.Y * Math.Sin(-angle) - transformOrigin.Y * delta.Y * Math.Sin(-angle);
                                y += delta.Y * Math.Cos(-angle) + transformOrigin.Y * delta.Y * (1 - Math.Cos(-angle));

                                x += delta.X * Math.Cos(angle) + transformOrigin.X * delta.X * (1 - Math.Cos(angle));
                                y += delta.X * Math.Sin(angle) - transformOrigin.X * delta.X * Math.Sin(angle);

                                w -= delta.X;
                                h -= delta.Y;
                                break;
                            }

                        case ResizeThumbPlacementEnum.TopCenter:
                            {
                                x += delta.Y * Math.Sin(-angle) - transformOrigin.Y * delta.Y * Math.Sin(-angle);
                                y += delta.Y * Math.Cos(-angle) + transformOrigin.Y * delta.Y * (1 - Math.Cos(-angle));

                                h -= delta.Y;
                                break;
                            }

                        case ResizeThumbPlacementEnum.RightTop:
                            {
                                x += delta.Y * Math.Sin(-angle) - transformOrigin.Y * delta.Y * Math.Sin(-angle);
                                y += delta.Y * Math.Cos(-angle) + transformOrigin.Y * delta.Y * (1 - Math.Cos(-angle));

                                x -= delta.X * transformOrigin.X * (1 - Math.Cos(angle));
                                y += delta.X * transformOrigin.X * Math.Sin(angle);

                                w += delta.X;
                                h -= delta.Y;
                                break;
                            }

                        case ResizeThumbPlacementEnum.RightCenter:
                            {
                                x -= delta.X * transformOrigin.X * (1 - Math.Cos(angle));
                                y += delta.X * transformOrigin.X * Math.Sin(angle);

                                w += delta.X;
                                break;
                            }

                        case ResizeThumbPlacementEnum.RightBottom:
                            {
                                x += delta.Y * transformOrigin.Y * Math.Sin(-angle);
                                y -= delta.Y * transformOrigin.Y * (1 - Math.Cos(-angle));

                                x -= delta.X * transformOrigin.X * (1 - Math.Cos(angle));
                                y += delta.X * transformOrigin.X * Math.Sin(angle);

                                w += delta.X;
                                h += delta.Y;
                                break;
                            }

                        case ResizeThumbPlacementEnum.BottomCenter:
                            {
                                x += delta.Y * transformOrigin.Y * Math.Sin(-angle);
                                y -= delta.Y * transformOrigin.Y * (1 - Math.Cos(-angle));

                                h += delta.Y;
                                break;
                            }

                        case ResizeThumbPlacementEnum.LeftBottom:
                            {
                                x += delta.Y * transformOrigin.Y * Math.Sin(-angle);
                                y -= delta.Y * transformOrigin.Y * (1 - Math.Cos(-angle));

                                x += delta.X * Math.Cos(angle) + transformOrigin.X * delta.X * (1 - Math.Cos(angle));
                                y += delta.X * Math.Sin(angle) - transformOrigin.X * delta.X * Math.Sin(angle);

                                w -= delta.X;
                                h += delta.Y;
                                break;
                            }

                        case ResizeThumbPlacementEnum.LeftCenter:
                            {
                                x += delta.X * Math.Cos(angle) + transformOrigin.X * delta.X * (1 - Math.Cos(angle));
                                y += delta.X * Math.Sin(angle) - transformOrigin.X * delta.X * Math.Sin(angle);

                                w -= delta.X;
                                break;
                            }

                        default:
                            {
                                break;
                            }
                    }

                    if (x.IsNormalNumber() && y.IsNormalNumber() && w.IsNormalNumber() && h.IsNormalNumber())
                    {
                        w = Math.Min(Target.MaxWidth, Math.Max(Target.MinWidth, w));
                        h = Math.Min(Target.MaxHeight, Math.Max(Target.MinHeight, h));
                        var rect = new Rect(x, y, w, h);
                        Target.Resize(rect);
                    }
                }
            }
        }


        protected override void OnDragCompleted(object sender, DragCompletedEventArgs e) { }
    }

}
