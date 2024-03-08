using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcuDiagSim.App.Helpers;
using EcuDiagSim.App.Interfaces;
using Microsoft.Extensions.Logging;
using Serilog.Formatting.Display;
using Serilog.Formatting;
using Serilog.Core;

namespace EcuDiagSim.App.ViewModels
{
    [ObservableObject]
    public partial class MainPageViewModel
    {
        public ILogger Logger; //public  is a hack to dynamically use the winui3 sink
        public ILoggerFactory LoggerFactory;
        private readonly IPathService _pathService;
        private readonly IApiWithAssociatedVciService _apiWithAssociatedVciService;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private CancellationToken _ct;

        [ObservableProperty] 
        [NotifyCanExecuteChangedFor(nameof(StopCommand))]
        [NotifyCanExecuteChangedFor(nameof(ClearCommand))]
        private bool _isRunning;

        [ObservableProperty] 
        [NotifyCanExecuteChangedFor(nameof(StartCommand))]
        private FileInfoViewModel[] _luaFileInfos;

        private string _luaWorkingDirectory = string.Empty;

        [ObservableProperty] 
        private string _state;

       [ObservableProperty] private bool _suppress3E7E;

        private bool CanRun { get; set; }
        public ItemsRepeaterLogBroker LogBroker { get; set; }

        public MainPageViewModel(ILoggerFactory loggerFactory, IPathService pathService, IApiWithAssociatedVciService apiWithAssociatedVciService)
        {
            Logger = loggerFactory.CreateLogger<MainPageViewModel>();
            LoggerFactory = loggerFactory;
            _pathService = pathService;
            _apiWithAssociatedVciService = apiWithAssociatedVciService;
            _state = "";
        }

        partial void OnSuppress3E7EChanged(bool value )
        {
            LogBroker.Suppress3e7e = value;
        }

        partial void OnLuaFileInfosChanged(FileInfoViewModel[] value)
        {
            if ( value.Any() )
            {
                CanRun = true;
            }
            else
            {
                CanRun = false;
            }
        }


        [RelayCommand]
        private void NewWorkingDirectory(string path)
        {
            UpdateLuaFileInfos(path);
            if ( !_luaWorkingDirectory.Equals(path) )
            {
                _luaWorkingDirectory = path;
                _pathService.SaveLuaWorkingDirectory(path);
            }
        }

        private void UpdateLuaFileInfos(string path)
        {
            FileInfo[] luaFiles;
            if ( File.Exists(path) )
            {
                // This path is a file
                var lua = new FileInfo[] { new(path) };
                LuaFileInfos = new FileInfoViewModel[] { new(new FileInfo(path)) };
            }
            else if ( Directory.Exists(path) )
            {
                // This path is a directory
                var d = new DirectoryInfo(path);
                var luas = d.GetFiles("*.lua", SearchOption.AllDirectories); //Getting lua files

                var tempFileInfoViewModels = new FileInfoViewModel[luas.Length];
                for ( var i = 0; i < luas.Length; i++ )
                {
                    tempFileInfoViewModels[i] = new FileInfoViewModel(luas[i]);
                }

                LuaFileInfos = tempFileInfoViewModels;
            }
            else
            {
                LuaFileInfos = new FileInfoViewModel[] {};
                Debug.WriteLine("{0} is not a valid file or directory.", path);
            }
        }

        [RelayCommand]
        private void Initialize()
        {
            if ( IsRunning )
            {
                return;
            }

            _luaWorkingDirectory = _pathService.LoadLuaWorkingDirectory() ?? "";
            UpdateLuaFileInfos(_luaWorkingDirectory);
        }

        [RelayCommand(AllowConcurrentExecutions = false, CanExecute = nameof(CanRun))]
        private async Task StartAsync()
        {
            _cts = new CancellationTokenSource();
            IsRunning = true;
            try
            {
                State = "Loading";

                Logger.LogInformation("Starting the ECU simulation");


                (string ApiName, string VciName)? vciOnApis = _apiWithAssociatedVciService.LoadVciOnApiSettings();
                if ( vciOnApis != null )
                {
                    var simTask = await Task.Factory.StartNew((async _ =>
                    {
                        using ( var manager = EcuDiagSimManagerFactory.Create(LoggerFactory, _luaWorkingDirectory,
                                   vciOnApis.GetValueOrDefault().ApiName,
                                   vciOnApis.GetValueOrDefault().VciName) )
                        {
                            manager.EcuDiagSimManagerEventLog += Manager_EcuDiagSimManagerEventLog;
                            if ( manager.EvaluateAndBuildSimEnv() )
                            {
                                await manager.ConnectAndRunAsync(_cts.Token);
                            }

                            manager.EcuDiagSimManagerEventLog -= Manager_EcuDiagSimManagerEventLog;
                        }
                    }), _cts.Token, TaskCreationOptions.LongRunning | TaskCreationOptions.RunContinuationsAsynchronously);
                    State = "Is Running";
                    await simTask;
                }

                Logger.LogInformation("ECU simulation finished");
            }
            catch ( Exception e )
            {
                State = "Is Stopping by Error !!";
                Logger.LogCritical(e, "ECU simulation finished with Error");
                try
                {
                    var tcs = new TaskCompletionSource();
                    await tcs.Task.WaitAsync(_cts.Token);
                }
                catch (TaskCanceledException) {/*nothing to do};*/}
            }
            finally
            {
                IsRunning = false;
                State = "Is Stopping";
                _cts.Dispose();
            }
        }

        private void Manager_EcuDiagSimManagerEventLog(object? sender, EventArgs e)
        {

        }

        [RelayCommand(AllowConcurrentExecutions = false, CanExecute = nameof(IsRunning))]
        private async Task StopAsync()
        {
            _cts.Cancel();
        }

        [RelayCommand(AllowConcurrentExecutions = false, CanExecute = nameof(IsRunning))]
        private async Task ClearAsync()
        {
            LogBroker.ClearUiLog();
        }

    }
}
