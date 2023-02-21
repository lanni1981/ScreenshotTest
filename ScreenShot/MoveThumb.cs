using System.Windows;
using System.Windows.Controls.Primitives;

namespace ScreenShot
{
    internal class MoveThumb : ThumbBase
    {
        /// <summary>
        /// 通常情况下，MoveThumb在目标的左(右、上、下)面以外，然而，有些目标可以通过拖动它的中心来移动。
        /// </summary>
        public bool IsMoveByTargetCenter
        {
            get
            {
                return System.Convert.ToBoolean(GetValue(IsMoveByTargetCenterProperty));
            }
            set
            {
                SetValue(IsMoveByTargetCenterProperty, value);
            }
        }

        public static readonly DependencyProperty IsMoveByTargetCenterProperty =
            DependencyProperty.Register("IsMoveByTargetCenter", typeof(bool), typeof(MoveThumb), new UIPropertyMetadata(false));

        protected override void OnDragStarted(object sender, DragStartedEventArgs e) { }

        protected override void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (Target != null)
            {
                var delta = new Point(e.HorizontalChange, e.VerticalChange);
                Target.Move(delta);
            }
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (IsMoveByTargetCenter)
            {
                var canvas = this.GetAncestor<MaskCanvas>();
                if (canvas != null) canvas.HandleIndicatorMouseDown(e);
            }
        }
    }
}
