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
//https://github.com/XamlBrewer/XamlBrewer.WinUI3.Navigation.Sample
//https://gist.github.com/mrjfalk/93f7c66cabce1f68fef7102c38b2505e
//https://github.com/microsoft/microsoft-ui-xaml/issues/4100
//https://www.youtube.com/watch?v=nKguHB3LD9A&list=PLWyJQIhN3vyOjJAdrURtKL7zDHFojN1sD&index=2
//https://stackoverflow.com/questions/71792862/c-sharp-winui-3-i-cannot-use-the-file-specified-from-a-filesavepicker-dialog-to
//https://www.davidbritch.com/2022/09/
//https://devblogs.microsoft.com/dotnet/announcing-the-dotnet-community-toolkit-810/
//https://github.com/microsoft/microsoft-ui-xaml/issues/3912
//https://learn.microsoft.com/en-us/windows/apps/develop/ui-input/retrieve-hwnd
//https://github.com/dotMorten/WinUIEx/
//https://learn.microsoft.com/en-us/windows/apps/winui/winui3/desktop-winui3-app-with-basic-interop
//https://devblogs.microsoft.com/dotnet/announcing-the-dotnet-community-toolkit-800/
//https://devblogs.microsoft.com/dotnet/announcing-the-dotnet-community-toolkit-810/

namespace EcuDiagSim.App
{
    public partial class App : Application
    {
        public static MainWindow MainWindow { get; private set; }

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
            MainWindow = Ioc.Default.GetRequiredService<MainWindow>();
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
                    .AddSingleton<IPathService, PathService>()
                    .AddSingleton<MainWindowViewModel>()
                    .AddSingleton<SettingsPageViewModel>()
                    .AddSingleton<MainPageViewModel>()
                    .AddSingleton<MainWindow>();
            })
            .Build();
    }
}