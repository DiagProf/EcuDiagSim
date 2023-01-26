using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using EcuDiagSim.App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using Windows.Storage;

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


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.InitializeCommand.Execute(this);
        }

        private async void MenuFlyoutItemSelectFile_OnClick(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            App.MainWindow.InitializeWithWindow(picker); //Win UI 3 Desktop Hack
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add(".lua");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                await ViewModel.NewWorkingDirectoryCommand.ExecuteAsync(file.Path);
            }
        }

        private async void MenuFlyoutItemSelectFolder_OnClick(object sender, RoutedEventArgs e)
        {
            var picker = new FolderPicker();
            App.MainWindow.InitializeWithWindow(picker); //Win UI 3 Desktop Hack
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add("*");
            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                ViewModel.NewWorkingDirectoryCommand.Execute(folder.Path);
            }
        }
    }
}