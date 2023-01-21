using CommunityToolkit.Mvvm.DependencyInjection;
using EcuDiagSim.App.Interfaces;
using EcuDiagSim.App.Services;
using EcuDiagSim.App.ViewModels;
using EcuDiagSim.App.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

//https://damienaicheh.github.io/uwp/2018/07/23/how-to-display-a-grouped-list-in-uwp-en
//https://stackoverflow.com/questions/72697312/show-object-details-using-winui-3-0-listview-with-grouped-headers
//https://xamlbrewer.wordpress.com/2022/02/07/building-a-master-detail-page-with-winui-3-and-mvvm/
//https://devblogs.microsoft.com/ifdef-windows/announcing-net-community-toolkit-v8-0-0-preview-1/
//https://devblogs.microsoft.com/dotnet/announcing-the-dotnet-community-toolkit-800/
//https://mariusbancila.ro/blog/2022/04/08/unwrapping-winui3-for-cpp/
//https://github.com/microsoft/WinUI-Gallery/tree/main

namespace EcuDiagSim.App
{
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            InitializeComponent();
            UnhandledException += App_UnhandledException;
            _host = BuildHost();
            Ioc.Default.ConfigureServices(_host.Services);
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);
            IAppActivationService appWindowService = Ioc.Default.GetRequiredService<IAppActivationService>();
            appWindowService.Activate(args);
        }

        private static IHost BuildHost() => Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                _ = services
                    .AddSingleton<ISettingsService, LocalSettingsService>()
                    .AddSingleton<IAppThemeService, AppThemeService>()
                    .AddSingleton<ILocalizationService, LocalizationService>()
                    .AddSingleton<IAppTitleBarService, AppTitleBarService>()
                    .AddSingleton<IWindowingService, WindowingService>()
                    .AddSingleton<INavigationViewService, NavigationViewService>()
                    .AddSingleton<IAppActivationService, AppActivationService>()
                    .AddSingleton<IApiWithAssociatedVciService, ApiWithAssociatedVciService>()
                    .AddSingleton<MainWindowViewModel>()
                    .AddSingleton<SettingsPageViewModel>()
                    .AddSingleton<MainPageViewModel>()
                    .AddSingleton<MainWindow>();
            })
            .Build();
    }
}