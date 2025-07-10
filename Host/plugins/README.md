# AutoCruz Plugins Directory

This directory contains dynamically loaded plugins for the AutoCruz Head Unit System.

## Directory Structure

```
plugins/
├── README.md                    # This file
├── AudioPlayerPlugin/           # Audio player plugin
│   ├── AudioPlayerPlugin.dll
│   ├── AudioPlayerPlugin.deps.json
│   └── config.json
├── VideoPlayerPlugin/           # Video player plugin
│   ├── VideoPlayerPlugin.dll
│   ├── VideoPlayerPlugin.deps.json
│   └── config.json
└── CustomPlugins/              # Third-party plugins
    └── [custom plugin folders]
```

## Plugin Loading

Plugins are automatically discovered and loaded at startup based on:

1. **Assembly Discovery**: All `.dll` files in subdirectories
2. **Interface Implementation**: Must implement `IPlugin` interface
3. **Configuration**: Each plugin can have its own `config.json`
4. **Dependencies**: Plugin dependencies are resolved automatically

## Plugin Configuration Format

Each plugin directory should contain a `config.json` file:

```json
{
  "Name": "Audio Player",
  "Version": "1.0.0",
  "Description": "Plays audio files",
  "Author": "AutoCruz Team",
  "Enabled": true,
  "Priority": 1,
  "Dependencies": [],
  "SupportedPlatforms": ["all"],
  "Settings": {
    "SupportedFormats": ["mp3", "wav", "flac"],
    "DefaultVolume": 50,
    "EnableEqualizer": true
  }
}
```

## Plugin Development

To create a new plugin:

1. Create a new Class Library project targeting .NET 9.0
2. Reference the `Core` project for `IPlugin` interface
3. Implement the `IPlugin` interface
4. Build and copy output to a subdirectory here
5. Add appropriate `config.json`

### Example Plugin Structure

```csharp
using Core.Models;

namespace MyCustomPlugin
{
    public class MyPlugin : IPlugin
    {
        public string Name => "My Custom Plugin";
        public string Description => "Does something amazing";
        
        // Implementation...
    }
}
```

## Security Notes

- Only load plugins from trusted sources
- Plugins run with full application privileges
- Plugin assemblies are loaded into the main application domain
- Malicious plugins can compromise system security

## Troubleshooting

### Plugin Not Loading
1. Check assembly dependencies are present
2. Verify `IPlugin` interface implementation
3. Check application logs for loading errors
4. Ensure plugin is enabled in configuration

### Plugin Crashes
1. Check plugin logs in `../logs/` directory
2. Verify plugin compatibility with current system version
3. Test plugin in isolation
4. Check for missing dependencies

### Performance Issues
1. Monitor plugin resource usage
2. Check for memory leaks in plugin code
3. Verify plugin priority settings
4. Consider disabling heavy plugins during development

## Plugin API Reference

See the Core project documentation for:
- `IPlugin` interface specification
- Available services and dependency injection
- Event system and messaging
- UI integration patterns
- Hardware access patterns (if applicable)