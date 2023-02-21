using System.Windows;
using System.Windows.Controls.Primitives;

namespace ScreenShot
{
    internal class ThumbBase : Thumb
    {
        public IndicatorObject? Target { get; set; }
        public ThumbBase()
        {
            FocusVisualStyle = null;
            DragStarted += OnDragStarted;
            DragDelta += OnDragDelta;
            DragCompleted += OnDragCompleted;
        }

        // 重写此方法来执行一些清理工作,并将当前操作添加到undo-redo管理器中
        protected virtual void OnDragCompleted(object sender, DragCompletedEventArgs e) { }

        // 重写此方法来拖动
        protected virtual void OnDragDelta(object sender, DragDeltaEventArgs e) { }

        // 重写此方法来为拖动做一些准备工作
        protected virtual void OnDragStarted(object sender, DragStartedEventArgs e) { }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            Target = this.GetAncestor<IndicatorObject>();
        }
    }

}
