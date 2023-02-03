using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using EcuDiagSim.App.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Storage.Pickers;
using Windows.Storage;
using EcuDiagSim.App.Helpers;
using Serilog.Core;
using Serilog.Templates;
using Serilog.Events;
using Serilog.Sinks.WinUi3.LogViewModels;
using System.Threading;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Sinks.File;
using Serilog.Sinks.WinUi3;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace EcuDiagSim.App.Views
{
    public sealed partial class MainPage : Page
    {
        public Serilog.ILogger MainSink;
        private ItemsRepeaterLogBroker _logBroker;

        public MainPage()
        {
            InitializeComponent();
            ViewModel = Ioc.Default.GetRequiredService<MainPageViewModel>();

            App.Current.Resources.TryGetValue("DefaultTextForegroundThemeBrush", out object defaultTextForegroundBrush);

            _logBroker = new ItemsRepeaterLogBroker(
                LogViewer,
                LogScrollViewer,
                new EmojiLogViewModelBuilder((defaultTextForegroundBrush as SolidColorBrush)?.Color)

                    .SetTimestampFormat(new ExpressionTemplate("[{@t:yyyy-MM-dd HH:mm:ss.fff}]"))

                    .SetLevelsFormat(new ExpressionTemplate("{@l:u3}"))
                    .SetLevelForeground(LogEventLevel.Verbose, Colors.Gray)
                    .SetLevelForeground(LogEventLevel.Debug, Colors.Gray)
                    .SetLevelForeground(LogEventLevel.Warning, Colors.Yellow)
                    .SetLevelForeground(LogEventLevel.Error, Colors.Red)
                    .SetLevelForeground(LogEventLevel.Fatal, Colors.HotPink)

                    .SetSourceContextFormat(new ExpressionTemplate("{Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)}"))

                    .SetMessageFormat(new ExpressionTemplate("{@m}"))
                    .SetMessageForeground(LogEventLevel.Verbose, Colors.Gray)
                    .SetMessageForeground(LogEventLevel.Debug, Colors.Gray)
                    .SetMessageForeground(LogEventLevel.Warning, Colors.Yellow)
                    .SetMessageForeground(LogEventLevel.Error, Colors.Red)
                    .SetMessageForeground(LogEventLevel.Fatal, Colors.HotPink)

                    .SetExceptionFormat(new ExpressionTemplate("{@x}"))
                    .SetExceptionForeground(Colors.HotPink)

            );
            MainSink = Log.Logger;

            var winUi3Sink= new LoggerConfiguration()
                .WriteTo.Logger(MainSink)
                .WriteTo.WinUi3Control(_logBroker)
                .CreateLogger();

            //Serilog ILogger assign to MS ILogger
            ViewModel.Logger = new SerilogLoggerFactory(winUi3Sink).CreateLogger<MainWindow>(); 
            
            _logBroker.IsAutoScrollOn = false;
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
                ViewModel.NewWorkingDirectoryCommand.Execute(file.Path);
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

        private void Page_Closed(object sender, RoutedEventArgs routedEventArgs)
        {
            //kick out WinUi3Control sink
            Log.Logger = MainSink;
            ViewModel.Logger = new SerilogLoggerFactory(MainSink).CreateLogger<MainWindow>();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
           // logtest.Error("fooFoo");
        }
    }
}