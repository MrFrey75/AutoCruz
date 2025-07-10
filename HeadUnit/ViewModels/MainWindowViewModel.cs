using System;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;

namespace HeadUnit.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string _statusMessage = "System Ready";
    private string _currentTime = "";
    private string _currentDate = "";
    private object? _currentView;
    
    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }
    
    public string CurrentTime
    {
        get => _currentTime;
        private set => this.RaiseAndSetIfChanged(ref _currentTime, value);
    }
    
    public string CurrentDate
    {
        get => _currentDate;
        private set => this.RaiseAndSetIfChanged(ref _currentDate, value);
    }
    
    public object? CurrentView
    {
        get => _currentView;
        private set => this.RaiseAndSetIfChanged(ref _currentView, value);
    }

    // Navigation Commands
    public ReactiveCommand<Unit, Unit> NavigateToHomeCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToAudioCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToVideoCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToSettingsCommand { get; }

    public MainWindowViewModel()
    {
        // Initialize commands
        NavigateToHomeCommand = ReactiveCommand.Create(NavigateToHome);
        NavigateToAudioCommand = ReactiveCommand.Create(NavigateToAudio);
        NavigateToVideoCommand = ReactiveCommand.Create(NavigateToVideo);
        NavigateToSettingsCommand = ReactiveCommand.Create(NavigateToSettings);
        
        // Start the clock
        StartClock();
        
        // Set default view
        NavigateToHome();
    }

    private void StartClock()
    {
        // Update time every second
        Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => UpdateDateTime());
    }

    private void UpdateDateTime()
    {
        var now = DateTime.Now;
        CurrentTime = now.ToString("HH:mm:ss");
        CurrentDate = now.ToString("MMM dd, yyyy");
    }

    private void NavigateToHome()
    {
        StatusMessage = "Home";
        CurrentView = new HomeViewModel();
    }

    private void NavigateToAudio()
    {
        StatusMessage = "Audio Player";
        CurrentView = new AudioViewModel();
    }

    private void NavigateToVideo()
    {
        StatusMessage = "Video Player";
        CurrentView = new VideoViewModel();
    }

    private void NavigateToSettings()
    {
        StatusMessage = "Settings";
        CurrentView = new SettingsViewModel();
    }
}

// Placeholder ViewModels for different views
public class HomeViewModel : ViewModelBase
{
    public string WelcomeMessage { get; } = "Welcome to Auto Cruz";
    public string SubMessage { get; } = "Your premium car entertainment system";
}

public class AudioViewModel : ViewModelBase
{
    public string Title { get; } = "Audio Player";
    public string Status { get; } = "No audio playing";
}

public class VideoViewModel : ViewModelBase
{
    public string Title { get; } = "Video Player";
    public string Status { get; } = "No video playing";
}

public class SettingsViewModel : ViewModelBase
{
    public string Title { get; } = "System Settings";
    public string Version { get; } = "Auto Cruz v1.0";
}