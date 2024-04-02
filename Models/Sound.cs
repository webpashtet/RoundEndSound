using System.Text.Json.Serialization;

namespace RoundEndSound.Models;

public class Sound
{
    [JsonPropertyName("name")] public string Name { get; init; } = "Test track";
    [JsonPropertyName("path")] public string Path { get; init; } = "sounds/music/mt_1.vsnd_c";
}