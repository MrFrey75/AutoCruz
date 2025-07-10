namespace Core.Models;

public interface IPlugin
{
    // Define the properties and methods that all plugins must implement
    string Name { get; }
    string Description { get; }

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
    
}