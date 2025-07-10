using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Avalonia;
using Avalonia.ReactiveUI;
using Core.Services;
using Core.Models;
using Hardware;
using HeadUnit;

namespace Host;

/// <summary>
/// Main entry point for AutoCruz Head Unit System
/// Orchestrates all system components including DI, configuration, hardware, and GUI
/// </summary>
class Program
{
    private static IServiceProvider? _serviceProvider;
    private static IHost? _host;
    private static CancellationTokenSource? _cancellationTokenSource;
    private static readonly object _lockObject = new();
    private static bool _shutdownInitiated = false;

    /// <summary>
    /// Application entry point
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Application exit code</returns>
    public static async Task<int> Main(string[] args)
    {
        // Setup global exception handling
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        _cancellationTokenSource = new CancellationTokenSource();
        
        // Setup console cancellation handler (Ctrl+C)
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            _cancellationTokenSource.Cancel();
        };

        try
        {
            PrintStartupBanner();
            
            // Build and configure the host with dependency injection
            _host = CreateHost(args);
            
            // Initialize core services
            await InitializeCoreServices(_cancellationTokenSource.Token);
            
            // Initialize hardware components
            await InitializeHardware(_cancellationTokenSource.Token);
            
            // Discover and load plugins
            await LoadPlugins(_cancellationTokenSource.Token);
            
            // Start the GUI application
            return await StartGuiApplication(args);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Application startup was cancelled");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error during startup: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return -1;
        }
        finally
        {
            await ShutdownServices();
        }
    }

    /// <summary>
    /// Creates and configures the dependency injection host
    /// </summary>
    private static IHost CreateHost(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? 
                              AppDomain.CurrentDomain.BaseDirectory;
                
                config.SetBasePath(basePath);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", 
                                  optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables("AUTOCRUZ_");
                config.AddCommandLine(args);
            })
            .ConfigureLogging((context, logging) =>
            {
                ConfigureLogging(logging, context.Configuration);
            })
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(services, context.Configuration);
            })
            .UseConsoleLifetime(); // Allows graceful shutdown

        return builder.Build();
    }

    /// <summary>
    /// Configures application logging
    /// </summary>
    private static void ConfigureLogging(ILoggingBuilder logging, IConfiguration configuration)
    {
        logging.ClearProviders();
        
        // Console logging
        logging.AddConsole();
        
        // Debug logging in development
#if DEBUG
        logging.AddDebug();
#endif
        
        // File logging setup
        var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        Directory.CreateDirectory(logPath);
        
        // Configure log levels from configuration
        logging.AddConfiguration(configuration.GetSection("Logging"));
        
        // Set default log level
        var logLevel = configuration.GetValue<LogLevel>("Logging:LogLevel:Default", LogLevel.Information);
        logging.SetMinimumLevel(logLevel);
    }

    /// <summary>
    /// Configures dependency injection services
    /// </summary>
    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Configuration objects
        services.Configure<AutoCruzSettings>(configuration.GetSection("AutoCruz"));
        services.Configure<HardwareSettings>(configuration.GetSection("Hardware"));
        services.Configure<PluginSettings>(configuration.GetSection("Plugins"));
        
        // Core Services
        services.AddSingleton<ConfigService>();
        services.AddSingleton<PluginService>();
        
        // Application Services
        services.AddSingleton<IPluginManager, PluginManager>();
        services.AddSingleton<IApplicationStateService, ApplicationStateService>();
        services.AddSingleton<ISystemMonitorService, SystemMonitorService>();
        
        // Hardware Services (conditionally registered based on configuration)
        var hardwareEnabled = configuration.GetValue<bool>("AutoCruz:EnableHardware", false);
        if (hardwareEnabled)
        {
            services.AddSingleton<ICanBusService, CanBusService>();
            services.AddSingleton<IGpioService, GpioService>();
            services.AddSingleton<IAudioHardwareService, AudioHardwareService>();
        }
        else
        {
            // Mock services for development
            services.AddSingleton<ICanBusService, MockCanBusService>();
            services.AddSingleton<IGpioService, MockGpioService>();
            services.AddSingleton<IAudioHardwareService, MockAudioHardwareService>();
        }
        
        // GUI Services
        services.AddTransient<HeadUnit.ViewModels.MainWindowViewModel>();
        
        // Health checks
        services.AddHealthChecks()
            .AddCheck<SystemHealthCheck>("system")
            .AddCheck<HardwareHealthCheck>("hardware")
            .AddCheck<PluginHealthCheck>("plugins");
    }

    /// <summary>
    /// Initializes core services before starting the GUI
    /// </summary>
    private static async Task InitializeCoreServices(CancellationToken cancellationToken)
    {
        _serviceProvider = _host!.Services;
        var logger = _serviceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Initializing core services...");
        
        try
        {
            // Initialize configuration service
            var configService = _serviceProvider.GetRequiredService<ConfigService>();
            await configService.InitializeAsync(cancellationToken);
            logger.LogInformation("✓ Configuration service initialized");
            
            // Initialize system monitor
            var systemMonitor = _serviceProvider.GetRequiredService<ISystemMonitorService>();
            await systemMonitor.StartAsync(cancellationToken);
            logger.LogInformation("✓ System monitor started");
            
            // Initialize application state
            var appStateService = _serviceProvider.GetRequiredService<IApplicationStateService>();
            await appStateService.InitializeAsync(cancellationToken);
            logger.LogInformation("✓ Application state service initialized");
            
            logger.LogInformation("Core services initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize core services");
            throw;
        }
    }

    /// <summary>
    /// Initializes hardware components
    /// </summary>
    private static async Task InitializeHardware(CancellationToken cancellationToken)
    {
        var logger = _serviceProvider!.GetRequiredService<ILogger<Program>>();
        var configuration = _serviceProvider.GetRequiredService<IConfiguration>();
        
        logger.LogInformation("Initializing hardware components...");
        
        try
        {
            var hardwareEnabled = configuration.GetValue<bool>("AutoCruz:EnableHardware", false);
            
            if (!hardwareEnabled)
            {
                logger.LogInformation("Hardware disabled in configuration - using mock services");
                return;
            }
            
            // Initialize CAN bus
            var canBus = _serviceProvider.GetService<ICanBusService>();
            if (canBus != null)
            {
                await canBus.InitializeAsync(cancellationToken);
                logger.LogInformation("✓ CAN bus initialized");
            }
            
            // Initialize GPIO
            var gpio = _serviceProvider.GetService<IGpioService>();
            if (gpio != null)
            {
                await gpio.InitializeAsync(cancellationToken);
                logger.LogInformation("✓ GPIO service initialized");
            }
            
            // Initialize audio hardware
            var audioHardware = _serviceProvider.GetService<IAudioHardwareService>();
            if (audioHardware != null)
            {
                await audioHardware.InitializeAsync(cancellationToken);
                logger.LogInformation("✓ Audio hardware initialized");
            }
            
            logger.LogInformation("Hardware initialization completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Hardware initialization failed");
            logger.LogWarning("Continuing without full hardware support");
        }
    }

    /// <summary>
    /// Discovers and loads plugins
    /// </summary>
    private static async Task LoadPlugins(CancellationToken cancellationToken)
    {
        var logger = _serviceProvider!.GetRequiredService<ILogger<Program>>();
        var configuration = _serviceProvider.GetRequiredService<IConfiguration>();
        
        logger.LogInformation("Loading plugins...");
        
        try
        {
            var pluginsEnabled = configuration.GetValue<bool>("AutoCruz:EnablePlugins", true);
            
            if (!pluginsEnabled)
            {
                logger.LogInformation("Plugins disabled in configuration");
                return;
            }
            
            var pluginManager = _serviceProvider.GetRequiredService<IPluginManager>();
            await pluginManager.DiscoverPluginsAsync(cancellationToken);
            
            logger.LogInformation($"✓ Plugin discovery completed. Found {pluginManager.LoadedPlugins.Count} plugins");
            
            foreach (var plugin in pluginManager.LoadedPlugins)
            {
                logger.LogInformation($"  - {plugin.Name}: {plugin.Description}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Plugin loading failed");
            throw;
        }
    }

    /// <summary>
    /// Starts the Avalonia GUI application
    /// </summary>
    private static async Task<int> StartGuiApplication(string[] args)
    {
        var logger = _serviceProvider!.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Starting GUI application...");
        
        try
        {
            // Configure Avalonia with our service provider
            var appBuilder = BuildAvaloniaApp()
                .AfterSetup(_ =>
                {
                    // Make services available to the GUI
                    if (Application.Current is App app)
                    {
                        app.Services = _serviceProvider;
                    }
                });
            
            // Start the host services in background
            _ = Task.Run(async () =>
            {
                try
                {
                    await _host!.RunAsync(_cancellationTokenSource!.Token);
                }
                catch (OperationCanceledException)
                {
                    // Expected when shutting down
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Host services failed");
                }
            }, _cancellationTokenSource!.Token);
            
            logger.LogInformation("✓ AutoCruz Head Unit System ready");
            
            // Run the GUI on the main thread
            return appBuilder.StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GUI application failed to start");
            throw;
        }
    }

    /// <summary>
    /// Configures the Avalonia application
    /// </summary>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();

    /// <summary>
    /// Graceful shutdown of all services
    /// </summary>
    private static async Task ShutdownServices()
    {
        lock (_lockObject)
        {
            if (_shutdownInitiated) return;
            _shutdownInitiated = true;
        }
        
        var logger = _serviceProvider?.GetService<ILogger<Program>>();
        
        try
        {
            logger?.LogInformation("Shutting down AutoCruz system...");
            
            // Cancel any ongoing operations
            _cancellationTokenSource?.Cancel();
            
            // Stop background services
            if (_serviceProvider != null)
            {
                var systemMonitor = _serviceProvider.GetService<ISystemMonitorService>();
                if (systemMonitor != null)
                {
                    await systemMonitor.StopAsync();
                    logger?.LogInformation("✓ System monitor stopped");
                }
            }
            
            // Stop the host and dispose services
            if (_host != null)
            {
                await _host.StopAsync(TimeSpan.FromSeconds(5));
                _host.Dispose();
                logger?.LogInformation("✓ Host services stopped");
            }
            
            _cancellationTokenSource?.Dispose();
            
            logger?.LogInformation("AutoCruz system shutdown completed");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error during shutdown");
        }
    }

    /// <summary>
    /// Prints startup banner
    /// </summary>
    private static void PrintStartupBanner()
    {
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════╗");
        Console.WriteLine("║            AUTO CRUZ                ║");
        Console.WriteLine("║     Car Entertainment System        ║");
        Console.WriteLine("║          Starting up...             ║");
        Console.WriteLine("╚══════════════════════════════════════╝");
        Console.WriteLine();
    }

    /// <summary>
    /// Global unhandled exception handler
    /// </summary>
    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var logger = _serviceProvider?.GetService<ILogger<Program>>();
        var ex = e.ExceptionObject as Exception;
        
        logger?.LogCritical(ex, "Unhandled exception occurred");
        Console.WriteLine($"FATAL: Unhandled exception: {ex?.Message}");
        
        if (e.IsTerminating)
        {
            Console.WriteLine("Application is terminating...");
        }
    }

    /// <summary>
    /// Unobserved task exception handler
    /// </summary>
    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        var logger = _serviceProvider?.GetService<ILogger<Program>>();
        
        logger?.LogError(e.Exception, "Unobserved task exception");
        Console.WriteLine($"Unobserved task exception: {e.Exception.Message}");
        
        e.SetObserved(); // Prevent application crash
    }
}