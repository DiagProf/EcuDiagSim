using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcuDiagSim.App.Interfaces;

namespace EcuDiagSim.App.ViewModels
{
    [ObservableObject]
    public partial class MainPageViewModel
    {
        private readonly IPathService _pathService;
        private readonly IApiWithAssociatedVciService _apiWithAssociatedVciService;

        [ObservableProperty] 
        [NotifyCanExecuteChangedFor(nameof(StopCommand))]
        private bool _isRunning;

        [ObservableProperty] 
        [NotifyCanExecuteChangedFor(nameof(StartCommand))]
        private FileInfoViewModel[] _luaFileInfos;

        private string _luaWorkingDirectory = string.Empty;

        [ObservableProperty] 
        private string _state;

        private bool CanRun { get; set; }

        public MainPageViewModel(IPathService pathService, IApiWithAssociatedVciService apiWithAssociatedVciService)
        {
            _pathService = pathService;
            _apiWithAssociatedVciService = apiWithAssociatedVciService;
            _state = "";
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
            IsRunning = true;
            try
            {
                State = "Is Running";
                await Task.Delay(15000);
            }
            finally
            {
                IsRunning = false;
            }
        }

        [RelayCommand(AllowConcurrentExecutions = false, CanExecute = nameof(IsRunning))]
        private async Task StopAsync()
        {
            IsRunning = false;
            State = "Is Stopping";
            await Task.Delay(43);
        }
    }
}
