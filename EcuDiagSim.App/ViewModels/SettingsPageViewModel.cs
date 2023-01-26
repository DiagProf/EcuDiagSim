using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using EcuDiagSim.App.Interfaces;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
//using CommunityToolkit.Common.Collections;
using CommunityToolkit.Mvvm.Collections;
using EcuDiagSim.App.Definitions;
using Microsoft.UI.Dispatching;

namespace EcuDiagSim.App.ViewModels
{
    [ObservableObject]
    public partial class SettingsPageViewModel
    {
        private readonly IWindowingService _windowingService;
        private readonly IAppTitleBarService _appTitleBarService;
        private readonly IAppThemeService _appThemeService;
        private readonly ILocalizationService _localizationService;
        private readonly IApiWithAssociatedVciService _apisWithCorrespondingVcisService;


        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UsedVciTwoLineButtonName))]
        private VciViewModel _lastUsedVci;

        public string UsedVciTwoLineButtonName => $"{_lastUsedVci.ApiShortName}\n{_lastUsedVci.VciName}";

        [ObservableProperty] 
        private ObservableGroupedCollection<ApiForVehicleCommunicationViewModel, VciViewModel> _availableVciOnApis = new();

        [ObservableProperty] 
        private ObservableGroup<ApiForVehicleCommunicationViewModel, VciViewModel> _vcisPerApi;


        [ObservableProperty]
        private int _windowWidth;

        [ObservableProperty]
        private int _windowHeight;

        [ObservableProperty]
        private Color _titleBarBackground;

        [ObservableProperty]
        private Color _titleBarForeground;

        [ObservableProperty]
        private IEnumerable<ThemeViewModel> _availableAppThemes;

        [ObservableProperty]
        private ThemeViewModel? _appTheme;

        [ObservableProperty]
        private IEnumerable<LanguageViewModel> _availableLanguages;

        [ObservableProperty]
        private LanguageViewModel? _language;

        [ObservableProperty]
        private bool _isLocalizationChanged = false;

      

        public SettingsPageViewModel(
            IWindowingService windowingService,
            IAppTitleBarService appTitleBarService,
            IAppThemeService appThemeService,
            ILocalizationService localizationService,
            IApiWithAssociatedVciService apisWithCorrespondingVcisService)
        {
            
            _windowingService = windowingService;
            _appTitleBarService = appTitleBarService;
            _appThemeService = appThemeService;
            _localizationService = localizationService;
            _apisWithCorrespondingVcisService = apisWithCorrespondingVcisService;


            _availableAppThemes = _appThemeService.AvailableThemes
                .Select(theme => new ThemeViewModel(theme, theme.ToString().GetLocalized("Resources")));

            _availableLanguages = _localizationService.AvailableLanguages
                .Select(language => new LanguageViewModel(language, language.GetLocalized("Resources")));

            string? language = _localizationService.GetLanguage();

            if (language is not null && _availableLanguages.Count(i => i.Language == language) > 0)
            {
                _language = new LanguageViewModel(language, language.GetLocalized("Resources"));
            }

            (string ApiName, string VciName)? vciOnApis = _apisWithCorrespondingVcisService.LoadVciOnApiSettings();
            if (vciOnApis != null)
            {
                //last stored API/VCI in a dummy VciViewModel without VCI-State
                _lastUsedVci = new VciViewModel(VciApiType.ISO229002, vciOnApis.GetValueOrDefault().ApiName, vciOnApis.GetValueOrDefault().VciName, "");
            }
            else
            {
                //Dummy VciViewModel
                _lastUsedVci = new VciViewModel(VciApiType.ISO229002, "Not selected", "", "");
            }
        }


        partial void OnLastUsedVciChanged(VciViewModel value)
        {
            //_apisWithCorrespondingVcisService.SetVciOnApi(vciViewModel.ApiShortName, vciViewModel.VciName);
            _ = _apisWithCorrespondingVcisService.SaveVciOnApiSettings(value.ApiShortName, value.VciName);
        }
        partial void OnTitleBarBackgroundChanged(Color value)
        {
            _appTitleBarService.SetBackground(value);
            _ = _appTitleBarService.SaveBackgroundSettings(value);
        }

        partial void OnTitleBarForegroundChanged(Color value)
        {
            _appTitleBarService.SetForeground(value);
            _ = _appTitleBarService.SaveForegroundSettings(value);
        }

        partial void OnAppThemeChanged(ThemeViewModel? value)
        {
            if (value is not null)
            {
                _appThemeService.SetTheme(value.Theme);
            }
        }

        partial void OnLanguageChanged(LanguageViewModel? value)
        {
            if (value is not null && value.Language != _localizationService.GetLanguage())
            {
                _localizationService.SetLanguage(value.Language);
                IsLocalizationChanged = true;
            }
        }

        [RelayCommand]
        private async Task Initialize()
        {
            (int Width, int Height)? windowSize = _windowingService.LoadWindowSizeSettings();
            windowSize ??= _windowingService.GetWindowSize();

            if ( windowSize is not null )
            {
                WindowWidth = windowSize.Value.Width;
                WindowHeight = windowSize.Value.Height;
            }

            if ( _appTitleBarService.LoadBackgroundSettings() is Color background )
            {
                TitleBarBackground = background;
            }

            if ( _appTitleBarService.LoadForegroundSettings() is Color foreground )
            {
                TitleBarForeground = foreground;
            }

            ElementTheme? theme = _appThemeService.LoadThemeSettings();
            theme ??= _appThemeService.GetTheme();

            if ( _availableAppThemes.FirstOrDefault(item => item.Theme.Equals(theme)) is ThemeViewModel themeItemViewModel )
            {
                AppTheme = themeItemViewModel;
            }


            var dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            await Task.Run(async () =>
            {
                foreach ( var api in _apisWithCorrespondingVcisService.GetAllInstalledApisWithRelatedVcis() )
                {
                    var vciViewModels = new List<VciViewModel>();
                    foreach ( var vci in api )
                    {
                        vciViewModels.Add(new VciViewModel(api.Key.Api, api.Key.ApiShortName, vci.VciName, vci.VciState));
                    }

                    _vcisPerApi = new ObservableGroup<ApiForVehicleCommunicationViewModel, VciViewModel>(
                        new ApiForVehicleCommunicationViewModel(
                            api.Key.Api, api.Key.ApiShortName, api.Key.ApiSupplierName,
                            api.Key.ApiDescription, vciViewModels.Count),
                        vciViewModels);

                    await dispatcherQueue.EnqueueAsync(() => { AvailableVciOnApis.AddGroup(_vcisPerApi); }, DispatcherQueuePriority.High);
                    //await Task.Delay(5000);//For test only
                }

                await dispatcherQueue.EnqueueAsync(() =>
                {
                    //if (LastUsedVci == null)
                    //{
                        foreach (var api in AvailableVciOnApis)
                        {
                            if (api.Key.ApiShortName.Equals(LastUsedVci.ApiShortName))
                            {
                                foreach (var vci in api)
                                {
                                    if (vci.VciName.Equals(LastUsedVci.VciName))
                                    {
                                        LastUsedVci = vci;
                                        break;
                                    }
                                }
                            }
                        }
                    //}
                }, DispatcherQueuePriority.High);

            });
        }

        public VciViewModel SelectedVci { get; set; }

        [RelayCommand]
        private void LoadWindowSize()
        {
            if (_windowingService.GetWindowSize() is (int, int) windowSize)
            {
                WindowWidth = windowSize.Width;
                WindowHeight = windowSize.Height;
                _ = _windowingService.SaveWindowSizeSettings(WindowWidth, WindowHeight);
            }
        }

        [RelayCommand]
        private void UpdateWindowSize()
        {
            _windowingService.SetWindowSize(WindowWidth, WindowHeight);
            _ = _windowingService.SaveWindowSizeSettings(WindowWidth, WindowHeight);
        }

        [RelayCommand]
        private void ChangeTheme(string themeString)
        {
            if (Enum.TryParse(themeString, out ElementTheme theme) is true)
            {
                _appThemeService.SetTheme(theme);
                _ = _appThemeService.SaveThemeSettings(theme);
            }
        }
    }
}