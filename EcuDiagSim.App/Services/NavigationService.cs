using EcuDiagSim.App.Interfaces;
using EcuDiagSim.App.Views;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EcuDiagSim.App.Services
{
    public class NavigationViewService : INavigationViewService
    {
        private readonly List<(string Tag, Type PageType)> _navigationPages = new();
        private NavigationView? _navigationView;
        private Frame? _contentFrame;

        public void Initialize(NavigationView navigationView, Frame contentFrame)
        {
            if (_navigationView is not null)
            {
                _navigationView.SelectionChanged -= OnSelectionChanged;
                _navigationView.BackRequested -= OnBackRequested;
            }

            _navigationView = navigationView;
            _navigationView.SelectionChanged += OnSelectionChanged;
            _navigationView.BackRequested += OnBackRequested;

            if (_contentFrame is not null)
            {
                _contentFrame.Navigated -= OnNavigated;
                _contentFrame.NavigationFailed -= OnNavigationFailed;
            }

            _contentFrame = contentFrame;
            _contentFrame.Navigated += OnNavigated;
            _contentFrame.NavigationFailed += OnNavigationFailed;

            foreach (NavigationViewItem item in _navigationView.MenuItems)
            {
                if (item.Tag.ToString() is string itemTag &&
                    Type.GetType(itemTag) is Type type)
                {
                    _navigationPages.Add((itemTag, type));
                }
            }

            _navigationView.SelectedItem ??= _navigationView.MenuItems.FirstOrDefault();
        }

        public void NavigateTo(Type pageType, NavigationTransitionInfo navigationTransitionInfo)
        {
            NavigationEventArgsParameter parameter = new(this, pageType);
            _ = _contentFrame?.Navigate(pageType, parameter, navigationTransitionInfo);
        }

        private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected is true)
            {
                NavigateTo(typeof(SettingsPage), args.RecommendedNavigationTransitionInfo);
            }
            else if (args.SelectedItemContainer is NavigationViewItemBase item &&
                     item.Tag.ToString() is string itemTag)
            {
                Type pageType = _navigationPages.FirstOrDefault(p => p.Tag == itemTag).PageType;
                NavigateTo(pageType, args.RecommendedNavigationTransitionInfo);
            }
        }

        private void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args) => _ = TryGoBack();

        private bool TryGoBack()
        {
            if (_contentFrame?.CanGoBack is not true)
            {
                return false;
            }

            if (_navigationView?.IsPaneOpen is true &&
                (_navigationView.DisplayMode is NavigationViewDisplayMode.Compact ||
                 _navigationView.DisplayMode is NavigationViewDisplayMode.Minimal))
            {
                return false;
            }

            _contentFrame?.GoBack();

            return true;
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            if (_navigationView is not null && _contentFrame is not null)
            {
                if (Type.Equals(_contentFrame.SourcePageType, typeof(SettingsPage)) is true)
                {
                    _navigationView.SelectedItem = _navigationView.SettingsItem;
                }
                else if (_contentFrame.SourcePageType is not null)
                {
                    (string Tag, Type PageType) = _navigationPages.FirstOrDefault(p => p.PageType == e.SourcePageType);

                    _navigationView.SelectedItem = _navigationView.MenuItems
                        .OfType<NavigationViewItem>()
                        .First(n => n.Tag.Equals(Tag));
                }
            }
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e) => throw new NavigationException(e.SourcePageType.FullName);

        public class NavigationEventArgsParameter
        {
            public NavigationEventArgsParameter(NavigationViewService navigationViewService, Type pageType)
            {
                NavigationViewService = navigationViewService;
                PageType = pageType;
            }

            public NavigationViewService NavigationViewService { get; }

            public Type PageType { get; }
        }

        public class NavigationException : Exception
        {
            public NavigationException(string? destination) => Destination = destination;

            public string? Destination { get; }
        }
    }
}