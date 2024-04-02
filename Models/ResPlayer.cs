using System.Reflection;
using CounterStrikeSharp.API.Modules.Menu;

namespace RoundEndSound.Models;

public class ResPlayer
{
    public string? SteamId { get; set; }
    public bool SoundEnabled { get; set; }
    public bool ChatEnabled { get; set; }
    public IMenu? Menu { get; set; }

    public Dictionary<string, bool> Settings
    {
        get
        {
            return GetType().GetProperties().Where(p => p.PropertyType == typeof(bool))
                .ToDictionary(property => property.Name, property => (bool)property.GetValue(this, null)!);
        }
    }
    
    public void SetBoolProperty(string propName, object value)
    {
        PropertyInfo[] properties = GetType().GetProperties();
        
        if (string.IsNullOrEmpty(propName)) return;
        
        foreach (var property in properties.Where(p => p.PropertyType == typeof(bool) && p.Name.Equals(propName)))
        {
            property.SetValue(this, value, null);
        }
    }
}