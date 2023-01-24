using CommunityToolkit.Mvvm.DependencyInjection;
using EcuDiagSim.App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using WinRT.Interop;

namespace EcuDiagSim.App.Views
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            ViewModel = Ioc.Default.GetRequiredService<MainWindowViewModel>();
        }

        public void InitializeWithWindow(object target)
        {
            var handle = WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(target, handle);
        }


        public AppTitleBar TitleBar { get => AppTitleBar; }

        public NavigationView AppNavigationViewControl { get => AppNavigationView; }

        public Frame ContentFrameControl { get => ContentFrame; }

        public MainWindowViewModel ViewModel { get; }
    }
}