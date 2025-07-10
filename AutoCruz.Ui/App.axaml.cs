using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AutoCruz.Plugins;
using AutoCruz.Ui.ViewModels;
using AutoCruz.Ui.Views;

namespace AutoCruz.Ui;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var plugins = PluginLoader.LoadPlugins("plugins");

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(plugins)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}