using System;
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
        private string _luaWorkingDirectory = string.Empty;
        private bool _IsRunning = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ToggleStartStopCommand))]
        private FileInfoViewModel[] _luaFileInfos;


        partial void OnLuaFileInfosChanged(FileInfoViewModel[] value)
        {
            if ( value.Any())
            {
                CanRun = true;
            }
            else
            {
                CanRun = false;
            }
        }

        [ObservableProperty]
        private string _state;

        //[ObservableProperty]
        //public bool _isStartStopPossible;
        private bool CanRun { get; set; }

        public MainPageViewModel(IPathService pathService)
        {
            _pathService = pathService;
            _state = "";
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
            _luaWorkingDirectory = _pathService.LoadLuaWorkingDirectory() ?? "";
            UpdateLuaFileInfos(_luaWorkingDirectory);
        }

        [RelayCommand(AllowConcurrentExecutions = false, CanExecute = nameof(CanRun))]
        private async Task ToggleStartStopAsync()
        {
            if ( _IsRunning )
            {
            }
            else
            {
                
            }

            await Task.Delay(43);
        }
    }
}
