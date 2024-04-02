using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using RoundEndSound.Models;
using RoundEndSound.Repository;

namespace RoundEndSound.Config;

public class Config : BasePluginConfig
{
    [JsonPropertyName("db_config")]
    public Database DbConfig { get; init; } = new()
    {
        Host = "127.0.0.1",
        DbName = "testDb",
        User = "testUser",
        Password = "superduperpassword"
    };
    [JsonPropertyName("random_selection_mode")] public bool RandomSelectionMode { get; init; } = true;
    [JsonPropertyName("default_enable_music")] public bool DefaultEnableMusic { get; init; } = true;
    [JsonPropertyName("default_enable_notify")] public bool DefaultEnableNotify { get; init; } = true;
    [JsonPropertyName("disable_MVP_sound")] public bool DisableMvpSound { get; init; } = true;
    [JsonPropertyName("re_open_menu_after_click")] public bool ReOpenMenuAfterItemClick { get; init; } = true;
    [JsonPropertyName("music_list")] public List<Sound> MusicList { get; init; } = [new Sound(), new Sound(), new Sound()];
}