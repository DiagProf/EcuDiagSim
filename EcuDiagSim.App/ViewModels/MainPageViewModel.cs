using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcuDiagSim.App.Interfaces;
using EcuDiagSim.App.Services;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EcuDiagSim.App.ViewModels
{
    [ObservableObject]
    public partial class MainPageViewModel
    {
        private readonly IPathService _pathService;

        public MainPageViewModel(IPathService pathService)
        {
            _pathService = pathService;
            _luaWorkingDirectory = _pathService.LoadLuaWorkingDirectory() ?? "nothing selected";
        }

        [ObservableProperty] private string _luaWorkingDirectory;

        partial void OnLuaWorkingDirectoryChanged(string value)
        {
            _pathService.SaveLuaWorkingDirectory(value);
        }

        [RelayCommand]
        private async Task NewWorkingDirectory(string path)
        {
            LuaWorkingDirectory = path;
        }

        [RelayCommand]
       private async Task Initialize()
       {
       }
    }
}