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
           
            OutputTextBlock.Text = "";
          
            FileOpenPicker picker = new FileOpenPicker();

            App.MainWindow.InitializeWithWindow(picker);

            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".lua");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                OutputTextBlock.Text = "Picked lua: " + file.Name;
            }
            else
            {
                OutputTextBlock.Text = "Operation cancelled.";
            }
        }
    }
}