using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ScreenShot
{
    /// <summary>
    /// 模态对话框
    /// </summary>
    public interface IModalDialog
    {
        public event EventHandler<EventArgs> CancelEventHandler;
        public event EventHandler<Exception> ErrorEventHandler;
        void Sweep();
    }
    /// <summary>
    /// 模态对话框
    /// </summary>
    public interface IModalDialog<T> : IModalDialog
    {
        public event EventHandler<T> DoneEventHandler;
    }
    public class ScreenShot : IModalDialog<BitmapSource>
    {
        public event EventHandler<BitmapSource>? DoneEventHandler;
        public event EventHandler<EventArgs>? CancelEventHandler;
        public event EventHandler<Exception>? ErrorEventHandler;
        public void Sweep()
        {
            maskWin = null;
            DoneEventHandler = null;
            CancelEventHandler = null;
        }

        MaskWindow maskWin;
        internal void StartCaputre(int timeOutSeconds)
        {
            StartCaputre(timeOutSeconds, Size.Empty);
        }

        internal async void StartCaputre(int timeOutSeconds, Size defaultSize)
        {
            await Task.Delay(200);
            if (maskWin == null) maskWin = new MaskWindow(this);
            maskWin.Show(timeOutSeconds, defaultSize);
        }

        internal void OnScreenCaputred(object? sender, BitmapSource caputredBmp)
        {
            DoneEventHandler?.Invoke(sender, caputredBmp);
            Sweep();
        }

        internal void OnScreenCaputreCancelled(object? sender)
        {
            CancelEventHandler?.Invoke(sender, EventArgs.Empty);
            Sweep();
        }

        /// <summary>
        /// 屏幕截屏
        /// </summary>
        /// <param name="timeOutSec">自动返回的超时时间，单位秒</param>
        /// <returns></returns>
        public static Task<BitmapSource> ScreenShotAsync(int timeOutSec = 20)
        {
            var tcs = new TaskCompletionSource<BitmapSource>();
            var screenShot = new ScreenShot();
            try
            {
                screenShot.CancelEventHandler += (s, e) =>
                {
                    try { tcs.TrySetResult(null); }
                    catch (Exception ex) { tcs.TrySetException(ex); }
                };
                screenShot.DoneEventHandler += (s, e) =>
                {
                    try { tcs.TrySetResult(e); }
                    catch (Exception ex) { tcs.TrySetException(ex); }
                };
            }
            catch (Exception exception)
            {
                tcs.TrySetException(exception);
            }
            screenShot.StartCaputre(timeOutSec);
            return tcs.Task;
        }
    }

}
