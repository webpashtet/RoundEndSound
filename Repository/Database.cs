using System.Text.Json.Serialization;

namespace RoundEndSound.Repository;

public class Database
{
    [JsonPropertyName("host")] public required string Host { get; init; }
    [JsonPropertyName("database")] public required string DbName { get; init; }
    [JsonPropertyName("user")] public required string User { get; init; }
    [JsonPropertyName("password")] public required string Password { get; init; }
    [JsonPropertyName("port")] public int Port { get; init; } = 3306;
}