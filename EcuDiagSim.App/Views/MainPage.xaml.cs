using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using EcuDiagSim.App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;

namespace EcuDiagSim.App.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            ViewModel = Ioc.Default.GetRequiredService<MainPageViewModel>();
        }

        public MainPageViewModel ViewModel { get; }

        private async void MenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            // Clear previous returned file name, if it exists, between iterations of this scenario
            OutputTextBlock.Text = "";
            //var hwnd = ViewModel.hwd;
            FileOpenPicker openPicker = new FileOpenPicker();
            // var hwnd = WindowNative.GetWindowHandle(this);
            // Get the current window's HWND by passing in the Window object
            //var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            var mw = Ioc.Default.GetRequiredService<MainWindow>();
            var hwnd = WindowNative.GetWindowHandle(mw);
            // var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Current);

            // Associate the HWND with the file picker
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);

            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                // The StorageFile has read/write access to the picked file.
                // See the FileAccess sample for code that uses a StorageFile to read and write.
                OutputTextBlock.Text = "Picked photo: " + file.Name;
            }
            else
            {
                OutputTextBlock.Text = "Operation cancelled.";
            }
        }
    }
}