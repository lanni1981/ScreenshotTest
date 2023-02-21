using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Media;

namespace ScreenshotTest
{
    internal partial class ViewModel:ObservableObject
    {
        [ObservableProperty]
        ImageSource? imageSource;

        [RelayCommand]
        async void Go()
        {
            ImageSource = null;
            GC.Collect();
            ImageSource = await ScreenShot.ScreenShot.ScreenShotAsync();
        }
    }
}
