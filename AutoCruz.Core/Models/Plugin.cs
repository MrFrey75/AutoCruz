using Avalonia.Controls;
using Avalonia;

namespace AutoCruz.Core.Models;

public interface IPlugin
{
    string Name { get; }
    void Initialize();
    UserControl View { get; } // 🔁 changed from method to property
}


public class Plugin : IPlugin
{
    public string Name { get; private set; }
    public string Description { get; private set; }

    public Plugin(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public void Initialize()
    {
        Console.WriteLine($"Initializing plugin: {Name}");
    }

    public UserControl View => new UserControl
    {
        Content = new TextBlock
        {
            Text = $"[Plugin View] {Name}: {Description}",
            Margin = new Thickness(10)
        }
    };
}