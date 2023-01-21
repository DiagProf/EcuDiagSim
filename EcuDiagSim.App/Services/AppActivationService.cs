using EcuDiagSim.App.Interfaces;
using EcuDiagSim.App.ViewModels;
using EcuDiagSim.App.Views;
using Microsoft.UI.Xaml;

namespace EcuDiagSim.App.Services
{
    public class AppActivationService : IAppActivationService
    {
        private readonly MainWindow _mainWindow;
        private readonly IWindowingService _windowingService;
        private readonly IAppTitleBarService _appTitleBarService;
        private readonly INavigationViewService _navigationViewService;
        private readonly ISettingsService _settingsService;
        private readonly IAppThemeService _appThemeService;
        private readonly ILocalizationService _localizationService;

        public AppActivationService(
            MainWindow mainWindow,
            IWindowingService windowingService,
            IAppTitleBarService appTitleBarService,
            INavigationViewService navigationViewService,
            ISettingsService settingsService,
            IAppThemeService appThemeService,
            ILocalizationService localizationService)
        {
            _mainWindow = mainWindow;
            _windowingService = windowingService;
            _appTitleBarService = appTitleBarService;
            _navigationViewService = navigationViewService;
            _settingsService = settingsService;
            _appThemeService = appThemeService;
            _localizationService = localizationService;
        }

        public void Activate(object activationArgs)
        {
            InitializeServices();
            _mainWindow.Activate();
        }

        private void InitializeServices()
        {
            _windowingService.Initialize(_mainWindow);

            _appTitleBarService.Initialize(_mainWindow.TitleBar);

            _navigationViewService.Initialize(_mainWindow.AppNavigationViewControl, _mainWindow.ContentFrameControl);
           
            if (_mainWindow.Content is FrameworkElement rootElement)
            {
                _appThemeService.Initialize(rootElement);
            }
            
            _localizationService.Initialize();
        }
    }
}