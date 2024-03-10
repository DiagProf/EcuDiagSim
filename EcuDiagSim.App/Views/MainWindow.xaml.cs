using CommunityToolkit.Mvvm.DependencyInjection;
using EcuDiagSim.App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace EcuDiagSim.App.Views
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            AppTitleBar.Text = AppTitleBar.Text + $" ({RuntimeInformation.ProcessArchitecture.ToString().ToLower()})"; ;
            SetTitleBar(AppTitleBar);
            ViewModel = Ioc.Default.GetRequiredService<MainWindowViewModel>();
        }

        public void InitializeWithWindow(object target)
        {
            var handle = WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(target, handle);
        }


        public AppTitleBar TitleBar => AppTitleBar ;

        public NavigationView AppNavigationViewControl => AppNavigationView;

        public Frame ContentFrameControl => ContentFrame;

        public MainWindowViewModel ViewModel { get; }
    }
}