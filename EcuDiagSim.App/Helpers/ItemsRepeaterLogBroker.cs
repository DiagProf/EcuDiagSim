using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.WinUi3;
using Serilog.Sinks.WinUi3.LogViewModels;

namespace EcuDiagSim.App.Helpers;

public class ItemsRepeaterLogBroker : IWinUi3LogBroker
{
    private readonly ILogViewModelBuilder _logViewModelBuilder;

    public ItemsRepeaterLogBroker(
        ItemsRepeater itemsRepeater,
        ScrollViewer scrollViewer,
        ILogViewModelBuilder logViewModelBuilder)
    {
        itemsRepeater.SetBinding(ItemsRepeater.ItemsSourceProperty, new Binding() { Source = Logs });

        _logViewModelBuilder = logViewModelBuilder;

        DispatcherQueue = itemsRepeater.DispatcherQueue;
        AddLogEvent = LogEvent;
        Logs.CollectionChanged += ((sender, e) =>
        {
            if (IsAutoScrollOn is true && sender is ObservableCollection<ILogViewModel> collection)
            {
                scrollViewer.ChangeView(
                    horizontalOffset: 0,
                    verticalOffset: scrollViewer.ScrollableHeight,
                    zoomFactor: 1,
                    disableAnimation: true);
            }
        });
    }

    private void LogEvent(LogEvent logEvent)
    {
        Logs.Insert(0, _logViewModelBuilder.Build(logEvent));

        if ( Logs.Count > 40 )
        {
            Logs.RemoveAt(Logs.Count-1);
        }
    }

    public Action<LogEvent> AddLogEvent { get; }
    public DispatcherQueue DispatcherQueue { get; }
    public bool IsAutoScrollOn { get; set; }

    private ObservableCollection<ILogViewModel> Logs { get; init; } = new();
    //collection.Insert(0, item);
}
