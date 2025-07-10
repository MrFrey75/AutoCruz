using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Timers;
using AutoCruz.Core.Models;
using ReactiveUI;

namespace AutoCruz.Ui.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<IPlugin> Plugins { get; }

    public string Greeting => "SAMPLETEXT";

    private string _clock;
    public string Clock
    {
        get => _clock;
        set => this.RaiseAndSetIfChanged(ref _clock, value);
    }

    public MainWindowViewModel(IEnumerable<IPlugin> plugins)
    {
        Plugins = new ObservableCollection<IPlugin>(plugins);
        StartClock();
    }

    private void StartClock()
    {
        _clock = DateTime.Now.ToString("hh:mm tt");

        var timer = new Timer(1000);
        timer.Elapsed += (_, __) =>
        {
            Clock = DateTime.Now.ToString("hh:mm tt");
        };
        timer.Start();
    }
}