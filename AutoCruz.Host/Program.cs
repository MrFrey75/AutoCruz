using Avalonia;

namespace AutoCruz.Host;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<AutoCruz.Ui.App>()
            .UsePlatformDetect()
            .LogToTrace();
}