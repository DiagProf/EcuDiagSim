using CommunityToolkit.Mvvm.DependencyInjection;
using EcuDiagSim.App.ViewModels;
using Microsoft.UI.Xaml.Controls;

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
    }
}