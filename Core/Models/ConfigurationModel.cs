using System.ComponentModel.DataAnnotations;

namespace Core.Models;

/// <summary>
/// Main AutoCruz system configuration
/// </summary>
public class AutoCruzSettings
{
    [Required]
    public string Environment { get; set; } = "Development";
    
    public bool EnableHardware { get; set; } = true;
    public bool EnablePlugins { get; set; } = true;
    
    [Required]
    public string PluginDirectory { get; set; } = "plugins";
    
    [Required]
    public string DataDirectory { get; set; } = "data";
    
    [Required]
    public string ConfigDirectory { get; set; } = "config";
    
    public TimeSpan StartupTimeout { get; set; } = TimeSpan.FromMinutes(2);
    public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    [Required]
    public DisplaySettings Display { get; set; } = new();
    
    [Required]
    public AudioSettings Audio { get; set; } = new();
    
    [Required]
    public PerformanceSettings Performance { get; set; } = new();
    
    [Required]
    public SecuritySettings Security { get; set; } = new();
}

/// <summary>
/// Display and UI configuration
/// </summary>
public class DisplaySettings
{
    [Range(640, 7680)]
    public int Width { get; set; } = 1280;
    
    [Range(360, 4320)]
    public int Height { get; set; } = 720;
    
    public bool Fullscreen { get; set; } = false;
    public bool AlwaysOnTop { get; set; } = false;
    
    [Range(0, 100)]
    public int Brightness { get; set; } = 75;
    
    public bool NightMode { get; set; } = false;
    public bool AutoNightMode { get; set; } = true;
    
    [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$")]
    public string NightModeStartTime { get; set; } = "20:00";
    
    [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$")]
    public string NightModeEndTime { get; set; } = "07:00";
    
    public TimeSpan ScreenSaverTimeout { get; set; } = TimeSpan.FromMinutes(5);
    
    public TouchSensitivity TouchSensitivity { get; set; } = TouchSensitivity.Medium;
}

/// <summary>
/// Audio system configuration
/// </summary>
public class AudioSettings
{
    [Range(0, 100)]
    public int Volume { get; set; } = 50;
    
    [Range(0, 100)]
    public int MaxVolume { get; set; } = 100;
    
    [Required]
    public string Equalizer { get; set; } = "Default";
    
    public bool EnableDSP { get; set; } = true;
    
    [Range(8000, 192000)]
    public int SampleRate { get; set; } = 44100;
    
    [Range(64, 8192)]
    public int BufferSize { get; set; } = 512;
    
    public bool EnableSpatialAudio { get; set; } = false;
    
    [Range(0, 10000)]
    public int CrossfadeTime { get; set; } = 3000;
}

/// <summary>
/// System performance configuration
/// </summary>
public class PerformanceSettings
{
    public bool EnableGPUAcceleration { get; set; } = true;
    
    [Range(128, 8192)]
    public int MaxMemoryUsage { get; set; } = 512;
    
    public bool EnableCaching { get; set; } = true;
    
    [Range(10, 1000)]
    public int CacheSize { get; set; } = 100;
    
    public GCMode GCMode { get; set; } = GCMode.Workstation;
}

/// <summary>
/// Security and access configuration
/// </summary>
public class SecuritySettings
{
    public bool EnableSafeMode { get; set; } = false;
    public bool AllowSystemAccess { get; set; } = false;
    public bool RequireAuthentication { get; set; } = false;
    public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromHours(1);
}

/// <summary>
/// Hardware interface configuration
/// </summary>
public class HardwareSettings
{
    [Required]
    public CanBusSettings CanBus { get; set; } = new();
    
    [Required]
    public GpioSettings GPIO { get; set; } = new();
    
    [Required]
    public AudioHardwareSettings Audio { get; set; } = new();
    
    [Required]
    public DisplayHardwareSettings Display { get; set; } = new();
    
    [Required]
    public UsbSettings USB { get; set; } = new();
    
    [Required]
    public BluetoothSettings Bluetooth { get; set; } = new();
}

/// <summary>
/// CAN bus configuration
/// </summary>
public class CanBusSettings
{
    public bool Enabled { get; set; } = false;
    
    [Required]
    public string Interface { get; set; } = "can0";
    
    [Range(50000, 1000000)]
    public int Bitrate { get; set; } = 500000;
    
    [Range(1, 10)]
    public int RetryCount { get; set; } = 3;
    
    [Range(1000, 30000)]
    public int TimeoutMs { get; set; } = 5000;
    
    public List<string> FilterIds { get; set; } = new();
}

/// <summary>
/// GPIO configuration
/// </summary>
public class GpioSettings
{
    public bool Enabled { get; set; } = false;
    
    [Range(1, 40)]
    public int PowerButton { get; set; } = 18;
    
    [Range(1, 40)]
    public int StatusLED { get; set; } = 24;
    
    [Range(1, 40)]
    public int BacklightControl { get; set; } = 12;
    
    [Range(1, 40)]
    public int FanControl { get; set; } = 16;
    
    [Range(10, 1000)]
    public int DebounceTime { get; set; } = 50;
}

/// <summary>
/// Audio hardware configuration
/// </summary>
public class AudioHardwareSettings
{
    [Required]
    public string InputDevice { get; set; } = "default";
    
    [Required]
    public string OutputDevice { get; set; } = "default";
    
    [Range(8000, 192000)]
    public int SampleRate { get; set; } = 44100;
    
    [Range(1, 8)]
    public int Channels { get; set; } = 2;
    
    [Range(64, 8192)]
    public int BufferSize { get; set; } = 512;
    
    public bool EnableMicrophone { get; set; } = true;
    
    [Range(0, 100)]
    public int MicrophoneGain { get; set; } = 50;
}

/// <summary>
/// Display hardware configuration
/// </summary>
public class DisplayHardwareSettings
{
    [Required]
    public string Type { get; set; } = "HDMI";
    
    [Required]
    public string Resolution { get; set; } = "1280x720";
    
    [Range(30, 144)]
    public int RefreshRate { get; set; } = 60;
    
    [Range(8, 32)]
    public int ColorDepth { get; set; } = 24;
    
    [Range(0, 270)]
    public int Rotation { get; set; } = 0;
}

/// <summary>
/// USB configuration
/// </summary>
public class UsbSettings
{
    public bool AutoMount { get; set; } = true;
    public List<string> AllowedDeviceTypes { get; set; } = new() { "MassStorage", "AudioDevice" };
    
    [Range(1000, 60000)]
    public int ScanInterval { get; set; } = 5000;
}

/// <summary>
/// Bluetooth configuration
/// </summary>
public class BluetoothSettings
{
    public bool Enabled { get; set; } = false;
    public bool Discoverable { get; set; } = true;
    public bool AutoConnect { get; set; } = true;
    
    [StringLength(4, MinimumLength = 4)]
    public string PinCode { get; set; } = "0000";
    
    public List<string> SupportedProfiles { get; set; } = new() { "A2DP", "HFP", "AVRCP" };
}

/// <summary>
/// Plugin system configuration
/// </summary>
public class PluginSettings
{
    public Dictionary<string, PluginConfig> Plugins { get; set; } = new();
}

/// <summary>
/// Individual plugin configuration
/// </summary>
public class PluginConfig
{
    public bool Enabled { get; set; } = true;
    
    [Range(1, 100)]
    public int Priority { get; set; } = 50;
    
    public List<string> SupportedFormats { get; set; } = new();
    public Dictionary<string, object> Settings { get; set; } = new();
}

/// <summary>
/// System monitoring configuration
/// </summary>
public class SystemMonitoringSettings
{
    public bool Enabled { get; set; } = true;
    
    [Range(1000, 300000)]
    public int MonitoringInterval { get; set; } = 5000;
    
    [Required]
    public PerformanceCountersSettings PerformanceCounters { get; set; } = new();
    
    [Required]
    public AlertSettings Alerts { get; set; } = new();
    
    public bool LogPerformanceData { get; set; } = false;
}

/// <summary>
/// Performance counters configuration
/// </summary>
public class PerformanceCountersSettings
{
    public bool CPU { get; set; } = true;
    public bool Memory { get; set; } = true;
    public bool Disk { get; set; } = true;
    public bool Network { get; set; } = false;
    public bool Temperature { get; set; } = true;
}

/// <summary>
/// Alert thresholds configuration
/// </summary>
public class AlertSettings
{
    [Range(0, 100)]
    public int CPUThreshold { get; set; } = 80;
    
    [Range(0, 100)]
    public int MemoryThreshold { get; set; } = 85;
    
    [Range(0, 100)]
    public int TemperatureThreshold { get; set; } = 75;
    
    [Range(0, 100)]
    public int DiskSpaceThreshold { get; set; } = 90;
}

/// <summary>
/// Enumerations for configuration
/// </summary>
public enum TouchSensitivity
{
    Low,
    Medium,
    High
}

public enum GCMode
{
    Workstation,
    Server
}