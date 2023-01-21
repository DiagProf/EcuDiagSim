using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;

namespace EcuDiagSim.App.Interfaces
{
    public interface INavigationViewService
    {
        void Initialize(NavigationView navigationView, Frame contentFrame);

        void NavigateTo(Type pageType, NavigationTransitionInfo navigationTransitionInfo);
    }
}