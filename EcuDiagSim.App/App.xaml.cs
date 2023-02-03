using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using CommunityToolkit.Mvvm.DependencyInjection;
using EcuDiagSim.App.Helpers;
using EcuDiagSim.App.Interfaces;
using EcuDiagSim.App.Services;
using EcuDiagSim.App.ViewModels;
using EcuDiagSim.App.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using UnhandledExceptionEventArgs = Microsoft.UI.Xaml.UnhandledExceptionEventArgs;


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
//https://devblogs.microsoft.com/dotnet/announcing-dotnet-community-toolkit-v810-preview-1/
//https://devblogs.microsoft.com/dotnet/announcing-the-dotnet-community-toolkit-810/
//https://medium.com/swlh/simple-event-souring-with-c-ec1eff55ee9d
//https://www.youtube.com/@marioneugebauer501
//https://github.com/hassanhabib/LeVent
//https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines
//https://benfoster.io/blog/serilog-best-practices/
//https://www.youtube.com/watch?v=qS-vp626H-M
//https://stackoverflow.com/questions/52921966/unable-to-resolve-ilogger-from-microsoft-extensions-logging
//https://stackoverflow.com/questions/1594357/wpf-how-to-use-2-converters-in-1-binding
//https://stackoverflow.com/questions/71350624/serilog-ilogger-to-microsoft-ilogger
//https://stackoverflow.com/questions/65443870/can-a-serilog-ilogger-be-converted-to-a-microsoft-extensions-logging-ilogger
//https://stackoverflow.com/questions/35567814/is-it-possible-to-display-serilog-log-in-the-programs-gui
//https://stackoverflow.com/questions/73262877/channels-is-it-possible-to-broadcast-receive-same-message-by-multiple-consumers

namespace EcuDiagSim.App
{
    public partial class App : Application
    {
        private readonly IHost _host;
        public static MainWindow MainWindow { get; private set; }

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
            var appWindowService = Ioc.Default.GetRequiredService<IAppActivationService>();
            appWindowService.Activate(args);
            MainWindow = Ioc.Default.GetRequiredService<MainWindow>();
            MainWindow.Closed += (sender, eventArgs) => { _host.Dispose(); };
        }

        private static IHost BuildHost() => Host.CreateDefaultBuilder()
            .ConfigureHostConfiguration(builder => builder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", false, true)
                //Next two only if need... not just now
                //.AddJsonFile($"appsettings.json.{Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") ?? "Production"}.json", true)
                //.AddEnvironmentVariables()
            )
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
                    // Views and ViewModels
                    .AddSingleton<MainWindowViewModel>()
                    .AddSingleton<SettingsPageViewModel>()
                    .AddSingleton<MainPageViewModel>()
                    .AddSingleton<MainWindow>();
            })
            .UseSerilog((context, services, configuration) =>
                {
                    configuration
                        .ReadFrom.Configuration(context.Configuration) //reads the appsettings.json from host
#if DEBUG
                        .MinimumLevel.Verbose()
#endif
                        .Enrich.FromLogContext()
                        .Enrich.With(new ThreadIdEnricher());
                }
            )
            .Build();

        private class ThreadIdEnricher : ILogEventEnricher
        {
            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "ThreadId", Thread.CurrentThread.ManagedThreadId));
            }
        }
    }
}
