using System.Reflection;
using AutoCruz.Core.Models;

namespace AutoCruz.Plugins;

public static class PluginLoader
{
    public static List<IPlugin> LoadPlugins(string pluginDirectory)
    {
        var plugins = new List<IPlugin>();

        if (!Directory.Exists(pluginDirectory))
            return plugins;

        foreach (var file in Directory.GetFiles(pluginDirectory, "*.dll"))
        {
            try
            {
                var asm = Assembly.LoadFrom(file);
                var types = asm.GetTypes()
                    .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var type in types)
                {
                    if (Activator.CreateInstance(type) is IPlugin plugin)
                    {
                        plugin.Initialize();
                        plugins.Add(plugin);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PluginLoader] Failed to load {file}: {ex.Message}");
            }
        }

        return plugins;
    }
}